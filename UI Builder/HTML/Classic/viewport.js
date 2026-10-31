/* Unity's Main canvas uses 360 logical units; device pixels scale the whole UI once. */
(()=>{'use strict';
 const app=document.getElementById('app');
 const scale=()=>app.getBoundingClientRect().width/app.offsetWidth||1;
 const rect=target=>{
  const value=target?.getBoundingClientRect?target.getBoundingClientRect():target;
  const factor=scale();
  return {left:value.left/factor,top:value.top/factor,width:value.width/factor,height:value.height/factor};
 };
 window.classicViewport={scale,rect};
 document.addEventListener('keydown',event=>{if(event.key==='Tab')document.body.classList.add('keyboard-navigation');});
 document.addEventListener('pointerdown',()=>document.body.classList.remove('keyboard-navigation'));
})();
