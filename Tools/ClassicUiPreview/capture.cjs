/* Replace captures atomically: Windows image previews may map an existing PNG. */
const fs=require('node:fs');
module.exports=async function capture(page,path){const image=await page.screenshot();const temporary=path+'.capture.tmp';try{fs.writeFileSync(temporary,image);fs.renameSync(temporary,path);}finally{if(fs.existsSync(temporary))fs.unlinkSync(temporary);}};
