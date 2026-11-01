// Separate synthetic diagnostic; does not change the acceptance runner or its observers.
import {chromium} from '@playwright/test'
import {writeFileSync} from 'node:fs'
import {modernFixtures,ids} from '../tests/e2e/fixtures/modernization.ts'
const base=process.argv[2]||'http://127.0.0.1:5179',label=process.argv[3]||'diagnostic'
if(!/^[a-zA-Z0-9_-]+$/.test(label))throw Error('Use a simple diagnostic label.')
const b=await chromium.launch(),results=[]
try {for (const [name,path,anonymous] of [['home','/',true],['lists','/lists',false],['personal','/personal-lists/'+ids.personal,false],['shared','/shared-lists/'+ids.shared,false]]) {
 const context=await b.newContext({viewport:{width:390,height:844}}),p=await context.newPage(),cdp=await context.newCDPSession(p)
 await cdp.send('Network.enable');await cdp.send('Network.setCacheDisabled',{cacheDisabled:true});await cdp.send('Network.emulateNetworkConditions',{offline:false,latency:150,downloadThroughput:1600000/8,uploadThroughput:750000/8});await cdp.send('Emulation.setCPUThrottlingRate',{rate:4})
 await p.addInitScript(()=>{
  window.__diagnostic={lcp:[],longTasks:[],render:[]}
  new PerformanceObserver(list=>{for(const e of list.getEntries())window.__diagnostic.lcp.push({startTime:e.startTime,renderTime:e.renderTime,loadTime:e.loadTime,size:e.size,element:e.element?{tag:e.element.tagName,text:e.element.textContent?.slice(0,180),className:String(e.element.className)}:null})}).observe({type:'largest-contentful-paint',buffered:true})
  new PerformanceObserver(list=>{for(const e of list.getEntries())window.__diagnostic.longTasks.push({startTime:e.startTime,duration:e.duration})}).observe({type:'longtask',buffered:true})
  new MutationObserver(()=>{const h=document.querySelector('h1');if(h&&!window.__diagnostic.render.length)window.__diagnostic.render.push({heading:h.textContent,at:performance.now()})}).observe(document,{subtree:true,childList:true})
 })
 await modernFixtures(p,50)
 await p.goto(base+'/tests/visual/index.html?route='+encodeURIComponent(path)+(anonymous?'&anonymous':''))
 await p.getByRole('heading',{level:1,name:new RegExp({home:'La compra, mejor',lists:'Mis listas',personal:'Compra de la semana',shared:'Compra de casa'}[name])}).waitFor();await p.evaluate(()=>document.fonts.ready);await p.waitForTimeout(300)
 const result=await p.evaluate(()=>({...window.__diagnostic,resources:performance.getEntriesByType('resource').map(r=>({name:new URL(r.name).pathname,startTime:r.startTime,responseEnd:r.responseEnd,duration:r.duration,bytes:r.transferSize,kind:r.initiatorType})),font: getComputedStyle(document.querySelector('h1')).font}))
 results.push({route:name,...result});console.log(name,JSON.stringify(result.lcp.at(-1)));await context.close()
}}finally{await b.close()}
writeFileSync('../specs/011-web-iconography/evidence/lcp-'+label+'-diagnostic.json',JSON.stringify({kind:'diagnostic-not-acceptance-samples',source:label,cpu:'4x',network:'1.6Mbps,150ms',viewport:'390x844',routes:results},null,2)+'\n')
