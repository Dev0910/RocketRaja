/* Shared presentation of the user-selected Kaju Katli token. No currency mutation. */
(()=>{'use strict';
const source=new URL('assets/currency/kaju-katli.png',document.currentScript.src).href;
const escape=value=>String(value).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;');
function icon({decorative=false,className=''}={}){return `<img class="kk-icon ${escape(className)}" data-currency="kaju-katli" src="${source}" alt="${decorative?'':'Kaju Katli'}"${decorative?' aria-hidden="true"':''} decoding="async">`;}
function amount(value){return `<span class="kk-amount" role="img" aria-label="${escape(value)} Kaju Katli"><b class="kk-number" aria-hidden="true">${escape(value)}</b>${icon({decorative:true})}</span>`;}
function decorate(root){
 const walker=document.createTreeWalker(root,NodeFilter.SHOW_TEXT),nodes=[];
 while(walker.nextNode())if(/\bKK\b/.test(walker.currentNode.nodeValue)&&!walker.currentNode.parentElement.closest('script,style,textarea,option,svg,.kk-amount'))nodes.push(walker.currentNode);
 for(const node of nodes){
  const text=node.nodeValue,pattern=/(?:([+]?\d[\d,.]*)[ \t-]+)?\bKK\b/g,fragment=document.createDocumentFragment();let cursor=0;
  for(const match of text.matchAll(pattern)){
   fragment.append(document.createTextNode(text.slice(cursor,match.index)));
   const template=document.createElement('template');template.innerHTML=match[1]?amount(match[1]):icon();fragment.append(template.content);
   cursor=match.index+match[0].length;
  }
  fragment.append(document.createTextNode(text.slice(cursor)));node.replaceWith(fragment);
 }
}
window.classicCurrency={source,icon,amount,decorate};
})();
