/* Presentation adapter for the current saved Unity hierarchy. No gameplay or persistence. */
(()=>{'use strict';
 const reference=window.classicUnityReference;
 const nativeText=value=>String(value||'').replace(/<size[^>]*>|<\/size>/g,'').replace(/<sprite=0>/g,'Kaju Katli');
 function apply(layer){
  const route=layer.dataset.screen,layout=reference[route],card=layer.querySelector('.modal-card'),body=layer.querySelector('.modal-body');
  if(!layout)return;
  layer.classList.add('unity-dialog');layer.classList.toggle('unity-framed',layout.frame);
  const style=card.style;
  style.setProperty('--dialog-height',layout.height+'px');style.setProperty('--dialog-header',layout.header+'px');
  style.setProperty('--dialog-title-size',layout.titleSize+'px');style.setProperty('--dialog-title-weight',layout.titleWeight);
  style.setProperty('--dialog-title-spacing',layout.titleWeight===700?'.03em':'0');
  ['left','top','right','bottom'].forEach((part,i)=>style.setProperty('--body-'+part,layout.body[i]+'px'));
  body.querySelectorAll('.illustrated-note').forEach(note=>{
   note.querySelectorAll(':scope>.popup-emblem').forEach(e=>e.remove());
   const copy=note.querySelector(':scope>.popup-note-copy');if(copy)copy.replaceWith(...copy.childNodes);
   note.classList.remove('illustrated-note');
  });
  if(layout.emblem){
   body.querySelectorAll(':scope>.popup-emblem').forEach(e=>e.remove());
   const emblem=document.createElement('div');emblem.className='unity-emblem';emblem.setAttribute('aria-hidden','true');
   const image=document.createElement('img');image.src='assets/popup-refresh/'+layout.emblem;image.alt='';emblem.append(image);body.prepend(emblem);
  }
  const cards=[...body.querySelectorAll(':scope>.info-card')];
  cards.forEach((item,i)=>{
   const source=layout.cards[i];if(!source)return;
   item.style.setProperty('--note-height',source.height+'px');
   if(['Help','Privacy'].includes(route)){
    item.replaceChildren();const heading=document.createElement('h3');heading.textContent=source.title;
    const copy=document.createElement('p');copy.textContent=nativeText(source.copy);item.append(heading,copy);
   }
  });
  if(route==='Skills'){
   const note=layer.querySelector('.skills-reset-note');if(note)body.append(note);
  }
  if(route==='Settings'){
   const tutorial=body.querySelector('[data-param=TutorialHelp]'),reset=body.querySelector('[data-param=DeveloperReset]');
   if(tutorial&&reset)tutorial.before(reset);
  }
  if(route==='Naming'){
   const note=body.querySelector('.info-card'),input=body.querySelector('.name-input'),error=body.querySelector('#shop-name-error');
   if(note&&input){
    input.remove();error?.remove();note.innerHTML='<h3>Name your shop</h3><p>A name for your little corner of the festival.</p>';
    note.after(input);const help=document.createElement('p');help.id='shop-name-help';help.className='unity-name-help';help.textContent='Up to 24 characters.';input.after(help);
    if(error)help.after(error);input.setAttribute('aria-describedby','shop-name-help shop-name-error');
   }
  }
  if(route==='Guidance'){
   const note=body.querySelector('.info-card');if(note){
    const heading=note.querySelector('h3'),topic=document.createElement('div'),copy=document.createElement('div');
    topic.className='unity-guidance-topic';topic.textContent=heading?.textContent||'';heading?.remove();
    copy.className='unity-guidance-copy';copy.textContent=note.textContent;note.replaceWith(topic,copy);
   }
  }
 }
 window.classicUnityPresentation={apply};
})();
