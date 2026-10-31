/* Shared semantic components. Presentation only; commands remain in classic.js. */
(()=>{'use strict';
const asset=name=>'parity/assets/icons/'+name+'.png';
const fresh=name=>'assets/popup-refresh/'+name+'.png';
function image(src,cls){const img=document.createElement('img');img.src=src;img.alt='';img.className=cls;img.setAttribute('aria-hidden','true');return img;}
function ledger(card,src,index){const title=card.querySelector('h3'),copy=document.createElement('div');copy.innerHTML=card.innerHTML.replace(title.outerHTML,'');const lines=copy.innerHTML.split(/<br\s*\/?>/i).map(html=>{const e=document.createElement('div');e.innerHTML=html;return e.textContent.trim();}).filter(Boolean);card.replaceChildren(title,image(src,'popup-ledger-art'));card.classList.add('popup-ledger');if(index<2){const dl=document.createElement('dl');for(const line of lines){const split=line.indexOf(':'),dt=document.createElement('dt'),dd=document.createElement('dd');dt.textContent=split<0?line:line.slice(0,split);dd.textContent=split<0?'':line.slice(split+1).trim();dl.append(dt,dd);}card.append(dl);}else{const p=document.createElement('p');lines.forEach((line,i)=>{if(i)p.append(document.createElement('br'));p.append(document.createTextNode(line));});card.append(p);}}
function settings(layer){const rows=[...layer.querySelectorAll('.settings-row')];rows.forEach((row,i)=>{const e=document.createElement('span');e.className='settings-symbol';e.setAttribute('aria-hidden','true');e.append(image(fresh(['music','sound','motion'][i]),''));row.prepend(e);});const icons={Naming:'shop',Account:'cloud',Privacy:'shield',Help:'book'};for(const btn of layer.querySelectorAll('.modal-body>[data-action=Navigate]')){const name=icons[btn.dataset.param];if(!name)continue;const label=document.createElement('span');label.className='settings-label';label.textContent=btn.textContent;const art=document.createElement('span');art.className='settings-symbol';art.setAttribute('aria-hidden','true');art.append(image(fresh(name),''));const arrow=document.createElement('span');arrow.className='settings-chevron';arrow.textContent='›';arrow.setAttribute('aria-hidden','true');btn.classList.add('settings-link');btn.replaceChildren(art,label,arrow);}}
function decorate(root){
 const route=root.dataset.screen,body=root.querySelector('.modal-body');
 if(route){
  root.classList.add('bazaar-dialog');root.dataset.bazaarComponent='dialog';
  if(route==='Settings'){
   settings(root);
   const reset=body.querySelector('[data-param=DeveloperReset]'),tutorial=body.querySelector('[data-param=TutorialHelp]');if(reset&&tutorial)tutorial.before(reset);
  }
  if(route==='Statistics')body.querySelectorAll(':scope>.info-card').forEach((card,i)=>ledger(card,asset(['coins','legacy','shelf'][i]),i));
  const subtitle=root.querySelector('.skills-subtitle');if(subtitle){subtitle.className='bazaar-lead';body.prepend(subtitle);}
  if(route==='Skills'){const note=root.querySelector('.skills-reset-note');if(note)body.append(note);}
  if(route==='CardDetail')root.querySelector('.modal-head h2').textContent='Talent card';
 }
 root.querySelectorAll('.product-row').forEach(row=>{const level=row.querySelector('.product-tile-level'),copy=row.querySelector('.product-tile-copy');if(level&&copy)copy.append(level);});
 const aliases={'.skill-portrait':'skill-quick-art','.skill-row-copy':'skill-quick-copy','.skill-row-state':'skill-quick-state','.skill-effect':'skill-quick-effect','.skill-timing':'skill-quick-timing','.skill-row-actions':'skill-quick-actions','.skill-details':'skill-details-link'};
 for(const [selector,cls]of Object.entries(aliases))root.querySelectorAll(selector).forEach(el=>el.classList.add(cls));
 root.querySelectorAll('.skill-details').forEach(el=>el.classList.add('action','secondary'));
 root.querySelectorAll('.action,.settings-link,.ledger-claim').forEach(el=>{el.dataset.bazaarComponent='button';el.dataset.bazaarRole=el.classList.contains('maroon')?'destructive':el.classList.contains('secondary')?'secondary':'primary';});
 root.querySelectorAll('.info-card,.product-overview-card,.skill-effect-tile').forEach(el=>el.dataset.bazaarComponent='section');
 root.querySelectorAll('.sheet-row,.skill-quick-row,.ledger-row').forEach(el=>el.dataset.bazaarComponent='row');
}
window.bazaarComponents={decorate};
})();
