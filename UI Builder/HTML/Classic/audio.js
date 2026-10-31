/* Local synthesized preview effects. No audio downloads, economy, storage or SDK. */
(()=>{'use strict';
let context,master,musicMaster,compressor,gesture=false,sound=true,music=true,serial=0,cues=0,nextBeat=0,beat=0,musicNotes=0;
const voices=new Set(),musicVoices=new Set(),last=new Map();
function ensure(){
 if(!gesture||!sound&&!music||document.hidden)return false;
 const Audio=window.AudioContext||window.webkitAudioContext;if(!Audio)return false;
 try{
  if(!context){context=new Audio();master=context.createGain();master.gain.value=.18;
   compressor=context.createDynamicsCompressor();compressor.threshold.value=-18;compressor.ratio.value=6;
   musicMaster=context.createGain();musicMaster.gain.value=.1;
   master.connect(compressor);musicMaster.connect(compressor);compressor.connect(context.destination);}
  if(context.state==='suspended')context.resume().catch(()=>{});
  return context.state!=='closed';
 }catch{return false;}
}
function stop(pool=voices){for(const voice of pool){try{voice.stop();}catch{}}pool.clear();}
function tone(frequency,at,duration=.13,volume=.2,type='sine',musical=false){
 const pool=musical?musicVoices:voices;if(pool.size>=(musical?12:32))return;
 const oscillator=context.createOscillator(),gain=context.createGain();
 oscillator.type=type;oscillator.frequency.setValueAtTime(frequency,at);
 gain.gain.setValueAtTime(.0001,at);gain.gain.exponentialRampToValueAtTime(volume,at+.004);
 gain.gain.exponentialRampToValueAtTime(.0001,at+duration);
 oscillator.connect(gain);gain.connect(musical?musicMaster:master);pool.add(oscillator);
 oscillator.onended=()=>{pool.delete(oscillator);oscillator.disconnect();gain.disconnect();};
 oscillator.start(at);oscillator.stop(at+duration+.02);
}
function shimmer(at,volume=.07){
 if(voices.size>=32)return;
 const buffer=context.createBuffer(1,Math.ceil(context.sampleRate*.12),context.sampleRate),data=buffer.getChannelData(0);
 for(let i=0;i<data.length;i++)data[i]=(Math.random()*2-1)*(1-i/data.length)**2;
 const source=context.createBufferSource(),filter=context.createBiquadFilter(),gain=context.createGain();
 source.buffer=buffer;filter.type='highpass';filter.frequency.value=1700;gain.gain.value=volume;
 source.connect(filter);filter.connect(gain);gain.connect(master);voices.add(source);
 source.onended=()=>{voices.delete(source);source.disconnect();filter.disconnect();gain.disconnect();};source.start(at);
}
function play(kind='click'){
 if(!sound||!ensure())return false;
 const now=performance.now(),gap=kind==='tap'?55:kind==='click'?70:140;
 if(now-(last.get(kind)??-Infinity)<gap)return false;last.set(kind,now);cues++;
 const at=context.currentTime+.005;
 if(kind==='tap'){
  const note=[659.25,783.99,880,987.77][serial++%4];
  tone(note,at,.095,.18,'triangle');tone(note*2,at,.065,.035);return true;
 }
 const notes=kind==='close'?[587.33,440]:kind==='open'||kind==='navigate'?[392,523.25]:kind==='click'?[660]:
  kind==='legendary'||kind==='celebrate'?[523.25,659.25,783.99,1046.5]:kind==='rare'?[523.25,622.25,783.99]:[523.25,659.25,783.99];
 const step=['click','open','close','navigate'].includes(kind) ? .045 : .065;
 notes.forEach((note,i)=>{tone(note,at+i*step,step===.045?.09:.22,step===.045?.12:.2,'triangle');
  if(step!==.045)tone(note*2,at+i*step,.15,.035);});
 if(['legendary','celebrate','reveal','rare'].includes(kind))shimmer(at+.03);
 return true;
}
function configure(options={}){
 if('sound' in options&&sound!==!!options.sound){sound=!!options.sound;if(!sound)stop();}
 if('music' in options&&music!==!!options.music){music=!!options.music;if(!music){stop(musicVoices);nextBeat=0;}}
 if(gesture&&(sound||music))ensure();
 if(context&&master){master.gain.setValueAtTime(sound ? .18 : 0,context.currentTime);musicMaster.gain.setValueAtTime(music ? .1 : 0,context.currentTime);}
}
// A quiet pentatonic bell/flute phrase, with sparse low notes. Presentation only.
const melody=[523.25,0,587.33,659.25,783.99,0,659.25,587.33,523.25,0,440,0,392,440,523.25,0,
 659.25,0,783.99,880,783.99,0,659.25,587.33,523.25,440,392,0,440,0,523.25,0];
function scheduleMusic(){
 if(!gesture||!music||document.hidden||context?.state!=='running')return;
 if(!nextBeat||nextBeat<context.currentTime-.3)nextBeat=context.currentTime+.04;
 while(nextBeat<context.currentTime+.15){
  const note=melody[beat%melody.length];
  if(note){tone(note,nextBeat,.38,.1,'sine',true);musicNotes++;}
  if(beat%8===0)tone(beat%32<16?130.81:110,nextBeat,.7,.07,'triangle',true);
  beat++;nextBeat+=.36;
 }
}
function unlock(event){
 if(!event.isTrusted||event.type==='keydown'&&!['Enter',' '].includes(event.key))return;
 gesture=true;ensure();
}
document.addEventListener('pointerdown',unlock,true);document.addEventListener('keydown',unlock,true);
document.addEventListener('visibilitychange',()=>{
 if(document.hidden){stop();stop(musicVoices);nextBeat=0;if(context?.state==='running')context.suspend().catch(()=>{});}
 else ensure();
});
setInterval(scheduleMusic,100);
window.classicAudio={play,configure,getSnapshot:()=>({supported:!!(window.AudioContext||window.webkitAudioContext),unlocked:gesture,state:context?.state||'locked',sound,music,cues,musicNotes,activeMusicVoices:musicVoices.size,activeVoices:voices.size})};
})();
