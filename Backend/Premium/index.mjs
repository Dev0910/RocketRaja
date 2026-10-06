import { onRequest } from 'firebase-functions/v2/https';
import { onMessagePublished } from 'firebase-functions/v2/pubsub';
import { onInit } from 'firebase-functions/v2/core';
import { defineString } from 'firebase-functions/params';
import { initializeApp, getApps } from 'firebase-admin/app';
import { getAuth } from 'firebase-admin/auth';
import { getAppCheck } from 'firebase-admin/app-check';
import { getFirestore } from 'firebase-admin/firestore';
import { GoogleAuth } from 'google-auth-library';
import { PremiumAuthority } from './authority.mjs';
import { GooglePlayVerifier } from './google-play.mjs';
import { FirestoreLedgerStore } from './firestore-store.mjs';
import { PremiumRateLimiter } from './rate-limit.mjs';
import { premiumConfiguration } from './hosting-config.mjs';
import { createPremiumHandler } from './http-handler.mjs';
import { GoogleVoidedPurchaseVerifier, PremiumRefundNotifications, parsePlayNotification } from './refund-notifications.mjs';

const region=defineString('PREMIUM_REGION');
const serviceAccount=defineString('PREMIUM_SERVICE_ACCOUNT');
const packageName=defineString('PREMIUM_ANDROID_PACKAGE');
const appIds=defineString('PREMIUM_FIREBASE_APP_IDS_JSON');
const packIds=defineString('PREMIUM_PACK_IDS_JSON');
const refundTopic=defineString('PREMIUM_PLAY_NOTIFICATIONS_TOPIC');
let handler,refunds;

// No credentials, Firebase access or API requests occur during source discovery/import.
onInit(()=>{
  const config=premiumConfiguration({packageName:packageName.value(),appIds:appIds.value(),packIds:packIds.value()});
  const app=getApps()[0]??initializeApp();
  const store=new FirestoreLedgerStore(getFirestore(app));
  const publisher=new GoogleAuth({scopes:['https://www.googleapis.com/auth/androidpublisher']});
  const authority=new PremiumAuthority({store,play:new GooglePlayVerifier(()=>publisher.getAccessToken()),packageName:config.packageName,packs:config.packs});
  const rates=new PremiumRateLimiter(store);
  refunds=new PremiumRefundNotifications(authority,new GoogleVoidedPurchaseVerifier(()=>publisher.getAccessToken()),config.packageName);
  handler=createPremiumHandler({authority,allowedAppIds:config.allowedAppIds,
    verifyIdToken:(token,revocation)=>getAuth(app).verifyIdToken(token,revocation),
    verifyAppCheck:token=>getAppCheck(app).verifyToken(token),
    checkRate:(uid,route)=>rates.allow(uid,route)});
});

// Cloud Run invocation is public because the handler verifies Firebase account and
// app tokens itself. Authentication is mandatory on every supported route.
export const premiumApi=onRequest({region,serviceAccount,invoker:'public',cors:false,timeoutSeconds:30,memory:'256MiB',minInstances:0,maxInstances:2,concurrency:40},async(request,response)=>{
  if(!handler)return response.status(503).json({error:'service-unavailable'});
  return handler(request,response);
});

export const premiumPlayNotifications=onMessagePublished({topic:refundTopic,region,serviceAccount,retry:true,timeoutSeconds:300,memory:'256MiB',minInstances:0,maxInstances:1,concurrency:1},async event=>{
  if(!refunds)throw new Error('Premium refund service unavailable');
  const notice=parsePlayNotification(event.data?.message);
  if(notice)await refunds.handle(notice,event.data.message.messageId);
});
