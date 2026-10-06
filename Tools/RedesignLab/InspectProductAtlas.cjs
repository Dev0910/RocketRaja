// Read-only PNG alpha analysis. Returns sprite rectangles; never rewrites source pixels.
const zlib=require('node:zlib');
function decode(buffer){
 const width=buffer.readUInt32BE(16),height=buffer.readUInt32BE(20);
 if(buffer.toString('hex',0,8)!=='89504e470d0a1a0a'||buffer[24]!==8||buffer[25]!==6||buffer[28]!==0)throw Error('Expected noninterlaced 8-bit RGBA PNG');
 const chunks=[];for(let o=8;o<buffer.length;){const n=buffer.readUInt32BE(o),type=buffer.toString('ascii',o+4,o+8);if(type==='IDAT')chunks.push(buffer.subarray(o+8,o+8+n));o+=12+n;}
 const raw=zlib.inflateSync(Buffer.concat(chunks)),stride=width*4,pixels=Buffer.alloc(stride*height);
 const paeth=(a,b,c)=>{const p=a+b-c,pa=Math.abs(p-a),pb=Math.abs(p-b),pc=Math.abs(p-c);return pa<=pb&&pa<=pc?a:pb<=pc?b:c;};
 for(let y=0;y<height;y++){const filter=raw[y*(stride+1)];if(filter>4)throw Error('Invalid PNG filter');for(let x=0;x<stride;x++){const i=y*stride+x,a=x>=4?pixels[i-4]:0,b=y?pixels[i-stride]:0,c=y&&x>=4?pixels[i-stride-4]:0;pixels[i]=(raw[y*(stride+1)+1+x]+(filter===0?0:filter===1?a:filter===2?b:filter===3?Math.floor((a+b)/2):paeth(a,b,c)))&255;}}
 return {width,height,pixels};
}
module.exports=function inspect(buffer){
 const {width,height,pixels}=decode(buffer),rects=[];
 for(let column=0;column<2;column++){
  const x0=column*width/2,x1=x0+width/2,counts=Array.from({length:height},(_,y)=>{let count=0;for(let x=x0;x<x1;x++)if(pixels[(y*width+x)*4+3]>8)count++;return count;});
  let bestStart=-1,bestLength=0,start=-1;for(let y=Math.floor(height*.32);y<Math.ceil(height*.72);y++){if(counts[y]===0){if(start<0)start=y;if(y-start+1>bestLength){bestLength=y-start+1;bestStart=start;}}else start=-1;}
  if(bestLength<3)throw Error('No clean transparent row gap in column '+column);
  const split=bestStart+Math.floor(bestLength/2);
  for(let row=0;row<2;row++){
   let left=x1,top=height,right=-1,bottom=-1;for(let y=row?split:0;y<(row?height:split);y++)for(let x=x0;x<x1;x++)if(pixels[(y*width+x)*4+3]>8){left=Math.min(left,x);right=Math.max(right,x);top=Math.min(top,y);bottom=Math.max(bottom,y);}
   if(right<left)throw Error('Empty product region');left=Math.max(x0,left-3);right=Math.min(x1-1,right+3);top=Math.max(row?split:0,top-3);bottom=Math.min((row?height:split)-1,bottom+3);
   rects[row*2+column]={x:left,yFromTop:top,width:right-left+1,height:bottom-top+1};
  }
 }
 return rects;
};
module.exports.decode=decode;
