'use strict';
const $ = (selector, root = document) => root.querySelector(selector);
const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];
const icon = name => `<svg class="icon" aria-hidden="true"><use href="#i-${name}"/></svg>`;
const escapeHtml = text => String(text).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const money = cents => new Intl.NumberFormat('es-ES', {style:'currency', currency:'EUR'}).format(cents / 100);
const seeds = {
  casa: {title:'Compra de la semana',space:'CASA',items:[['Leche de avena','2 unidades',340,true],['Pan integral','1 unidad',210,false],['Tomates','1 kg',350,false],['Huevos camperos','12 unidades',380,true],['Aceite de oliva','1 litro',1200,false]]},
  piso: {title:'Compra del piso',space:'PISO COMPARTIDO',items:[['Papel de cocina','2 rollos',250,false],['Café molido','1 paquete',480,true],['Detergente','1 botella',620,false],['Pasta','2 paquetes',260,false]]},
  plan: {title:'Cena del sábado',space:'PLANES',items:[['Pasta fresca','3 paquetes',900,false],['Salsa de tomate','2 tarros',460,true],['Queso parmesano','1 cuña',540,false],['Bebidas','6 unidades',720,false]]}
};
let nextId = 20;
const makeItems = items => items.map((i,index)=>({id:index+1,name:i[0],quantity:i[1],cents:i[2],checked:i[3]}));
let heroItems = makeItems(seeds.casa.items);
let demo = {scenario:'casa',title:seeds.casa.title,space:seeds.casa.space,items:makeItems(seeds.casa.items),settled:false};
let selectedStep = 0;
let activeDemoTab = 'list';
let toastTimer;

function notify(message) {
  clearTimeout(toastTimer);
  $('#toast').hidden = true;
  const openDialogs = $$('dialog[open]');
  const currentDialog = openDialogs.at(-1);
  $$('.dialog-toast').forEach(el=>el.remove());
  if (currentDialog && !currentDialog.classList.contains('viewport-dialog')) {
    const notification = document.createElement('p');
    notification.className = 'dialog-toast';
    notification.setAttribute('role','status');
    notification.textContent = message;
    const target = currentDialog.id === 'demo-dialog' ? $('.demo-content') : $('#flow-content');
    target.append(notification);
    toastTimer = setTimeout(()=>notification.remove(), 4500);
  } else {
    const toast = $('#toast');
    toast.textContent = message;
    toast.hidden = false;
    toastTimer = setTimeout(()=>toast.hidden=true, 4500);
  }
}

function groceryRows(items, scope) {
  if (!items.length) return `<div class="empty-state"><span class="icon-tile">${icon('list')}</span><h3>Tu lista empieza contigo.</h3><p>Añade el primer producto arriba.<br>Después, podrás probar cómo se marca.</p></div>`;
  return items.map(item=>`<div class="grocery-row"><label class="grocery-check" for="${scope}-item-${item.id}"><input type="checkbox" id="${scope}-item-${item.id}" data-scope="${scope}" data-item="${item.id}" ${item.checked?'checked':''}><span class="check-box">${icon('check')}</span><span class="product-text"><span class="product-name">${escapeHtml(item.name)}</span><span class="product-quantity">${escapeHtml(item.quantity)}</span></span></label><span class="product-price">${item.cents?money(item.cents):'Sin precio'}</span></div>`).join('');
}

function renderHero() {
  $('#hero-list').innerHTML = groceryRows(heroItems,'hero');
  const checked = heroItems.filter(i=>i.checked).length;
  $('#hero-progress-text').textContent = `${checked} de ${heroItems.length} en la cesta`;
  $('#hero-progress-percent').textContent = `${Math.round(checked/heroItems.length*100)} %`;
  $('#hero-progress').value = checked;
}

function renderDemoList() {
  $('#demo-title').textContent = demo.title;
  $('#demo-space').textContent = `TU ESPACIO · ${demo.space}`;
  $('#demo-list').innerHTML = groceryRows(demo.items,'demo');
  const checked = demo.items.filter(i=>i.checked).length;
  $('#demo-counter').textContent = `${checked}/${demo.items.length}`;
  $('#demo-total').textContent = money(demo.items.reduce((sum,i)=>sum+i.cents,0));
  $('#demo-progress-text').textContent = `${checked} de ${demo.items.length}`;
  $('#demo-progress').max = demo.items.length || 1;
  $('#demo-progress').value = checked;
}

function renderExpenses() {
  const total = demo.items.reduce((sum,i)=>sum+i.cents,0);
  const base = Math.floor(total/3);
  const shares = [base+(total%3>0?1:0),base+(total%3>1?1:0),base];
  $('#demo-expenses-panel').innerHTML = `<div class="expense-layout"><div><div class="expense-head"><div><span>Compra de ejemplo · Pagada por ti</span><strong>${money(total)}</strong></div>${icon('wallet')}</div><div class="summary-row"><strong>Reparto a partes iguales</strong><span class="split-tag">3 personas</span></div>${['Tú','Ana','Dani'].map((name,i)=>`<div class="expense-person"><span class="avatar ${['avatar-teal','avatar-peach','avatar-purple'][i]}">${i===0?'Tú':name[0]}</span><span>${name}<small>${i===0?'Has adelantado la compra':demo.settled?'Saldado en este ejemplo':'Pendiente de aportar'}</small></span><strong>${money(shares[i])}</strong></div>`).join('')}<div class="expense-explanation"><strong>¿Cómo salen las cuentas?</strong>Se divide el total estimado de la lista entre tres. Los céntimos que sobran se reparten de uno en uno. Los productos sin precio no se suman.</div></div><aside class="settlement-card"><span class="balance-check">${icon('check')}</span><h3>${demo.settled?'Todo en orden.':'Cada uno, su parte.'}</h3><p>${demo.settled?'Has probado el estado de una compra saldada. Nadie tiene pagos pendientes en esta simulación.':total?`En este ejemplo, Ana te aportaría ${money(shares[1])} y Dani ${money(shares[2])}.`:'Esta lista todavía no tiene importes. Abre una compra de ejemplo para probar el reparto.'}</p><button class="button ${demo.settled?'secondary':'primary'} full-width" id="settle-expenses" ${!total?'disabled':''}>${demo.settled?'Volver a pendiente':'Simular compra saldada'} ${icon(demo.settled?'reset':'check')}</button><span class="settlement-state">Simulación · No se realiza ningún pago</span></aside></div>`;
  $('#settle-expenses').addEventListener('click',()=>{
    demo.settled = !demo.settled;
    renderExpenses();
    $('#settle-expenses').focus();
    notify(demo.settled?'Compra marcada como saldada en la demo.':'El reparto vuelve al estado pendiente.');
  });
}

function setDemoTab(tab, focus=false) {
  activeDemoTab = tab;
  $$('[data-demo-tab]').forEach(button=>{
    const selected = button.dataset.demoTab === tab;
    button.setAttribute('aria-selected',String(selected));
    button.tabIndex = selected?0:-1;
    if(selected&&focus)button.focus();
  });
  $('#demo-list-panel').hidden = tab!=='list';
  $('#demo-expenses-panel').hidden = tab!=='expenses';
  if(tab==='expenses')renderExpenses();
}

function openDemo(scenario=null, tab='list', invite=false) {
  if(scenario) {
    const seed = seeds[scenario];
    demo = {scenario,title:seed.title,space:seed.space,items:makeItems(seed.items),settled:false};
    if(scenario==='casa')demo.items=heroItems.map(i=>({...i}));
  }
  $('#flow-dialog').close();
  renderDemoList();
  setDemoTab(tab);
  $('#invite-panel').hidden = !invite;
  if(!$('#demo-dialog').open)$('#demo-dialog').showModal();
  if(invite)$('#copy-invite').focus();
}

function openFlow(mode) {
  closeMenu();
  const content = $('#flow-content');
  if(mode==='create') {
    content.innerHTML = `<span class="eyebrow">EL PRIMER PASO ES UNA LISTA</span><h2 id="flow-title" style="margin-top:14px">¿Qué vais a comprar?</h2><p>Dale un nombre y empieza por lo que falta.</p><form class="create-form" id="create-form" novalidate><label class="field-label" for="list-name">Nombre de la lista</label><input id="list-name" name="list-name" placeholder="Por ejemplo, La compra de casa" maxlength="60" required autocomplete="off" aria-describedby="name-error"><p class="form-error" id="name-error" hidden>Escribe un nombre para tu lista.</p><button class="button primary full-width" type="submit">Crear lista de prueba ${icon('arrow')}</button></form><p class="flow-footnote">Prototipo: la lista es temporal y no requiere una cuenta.</p><div class="flow-divider"><span>¿Prefieres empezar con algo?</span><button class="text-button" id="flow-example">Ver un ejemplo ${icon('arrow')}</button></div>`;
    $('#create-form').addEventListener('submit',e=>{
      e.preventDefault();
      const input = $('#list-name');
      const name = input.value.trim();
      if(!name){$('#name-error').hidden=false;input.setAttribute('aria-invalid','true');input.focus();return;}
      demo={scenario:'custom',title:name,space:'MI NUEVA LISTA',items:[],settled:false};
      openDemo();
      $('#new-item').focus();
    });
    $('#list-name').addEventListener('input',()=>{$('#name-error').hidden=true;$('#list-name').removeAttribute('aria-invalid');});
    $('#flow-example').addEventListener('click',()=>openDemo('casa'));
  } else {
    content.innerHTML = `<span class="eyebrow">QUÉ BIEN TENERTE POR AQUÍ</span><h2 id="flow-title" style="margin-top:14px">Tu próxima compra te espera.</h2><p>En la aplicación, entrarás con tu cuenta de Microsoft para volver a tus listas.</p><div class="login-info">${icon('info')}<span>Esta es una vista de prueba. Puedes explorar la experiencia sin iniciar sesión ni introducir datos personales.</span></div><button class="button primary full-width" id="login-demo"><span class="microsoft-icon" aria-hidden="true"><i></i><i></i><i></i><i></i></span> Simular acceso con Microsoft</button><p class="flow-footnote">Se abrirá una lista con datos de ejemplo.</p><div class="flow-divider"><span>¿Empiezas desde cero?</span><button class="text-button" id="login-create">Crear una lista ${icon('arrow')}</button></div>`;
    $('#login-demo').addEventListener('click',()=>openDemo('casa'));
    $('#login-create').addEventListener('click',()=>openFlow('create'));
  }
  if(!$('#flow-dialog').open)$('#flow-dialog').showModal();
  if(mode==='create')$('#list-name').focus();
}

function setStep(index, focus=false) {
  selectedStep = index;
  $$('[data-step]').forEach(button=>{
    const active=Number(button.dataset.step)===index;
    button.classList.toggle('active',active);
    button.setAttribute('aria-selected',String(active));
    button.tabIndex=active?0:-1;
    if(active&&focus)button.focus();
  });
  $('#step-panel').setAttribute('aria-labelledby',`step-${index}`);
  const panels=[
    `<div class="mini-demo-card"><span class="overline">NUEVA LISTA</span><h3>Un poco de organización.</h3><div class="mini-row"><span>La compra de casa</span>${icon('list')}</div><div class="mini-input">${icon('plus')} Leche, pan, algo para cenar…</div></div><button class="text-button" data-action="create">Crea tu primera lista ${icon('arrow')}</button>`,
    `<div class="mini-demo-card"><span class="overline">UNA LISTA COMPARTIDA</span><h3>Todos en el mismo plan.</h3><div class="mini-people"><span class="avatar avatar-teal">Tú</span><span class="avatar avatar-peach">A</span><span class="avatar avatar-purple">D</span></div><p class="invite-status"><strong>Tú, Ana y Dani</strong>Lo que falta y lo que ya está, a la vista.</p></div><button class="text-button" data-action="invite">Prueba una invitación ${icon('arrow')}</button>`,
    `<div class="mini-demo-card"><span class="overline">COMPRA DE EJEMPLO · 24,80 €</span><h3>Compartir también las cuentas.</h3><div class="mini-row"><span>Tu parte</span><strong>8,27 €</strong></div><div class="mini-row"><span>La parte de Ana</span><strong>8,27 €</strong></div><div class="mini-row"><span>La parte de Dani</span><strong>8,26 €</strong></div></div><button class="text-button" data-action="expenses">Explora el reparto ${icon('arrow')}</button>`
  ];
  $('#step-panel').innerHTML=panels[index];
}

function closeMenu(){
  $('#mobile-nav').hidden=true;
  $('.menu-toggle').setAttribute('aria-expanded','false');
  $('.menu-toggle').setAttribute('aria-label','Abrir menú');
}

document.addEventListener('click',event=>{
  const actionButton=event.target.closest('[data-action]');
  if(actionButton){
    const action=actionButton.dataset.action;
    if(action==='create'||action==='login')openFlow(action);
    if(action==='demo')openDemo('casa');
    if(action==='invite')openDemo('casa','list',true);
    if(action==='expenses')openDemo('casa','expenses');
  }
  const scenarioButton=event.target.closest('[data-scenario]');
  if(scenarioButton)openDemo(scenarioButton.dataset.scenario);
  const closeButton=event.target.closest('.close-dialog');
  if(closeButton)closeButton.closest('dialog').close();
});

document.addEventListener('change',event=>{
  if(!event.target.matches('[data-item]'))return;
  const input=event.target;
  const items=input.dataset.scope==='hero'?heroItems:demo.items;
  const item=items.find(i=>i.id===Number(input.dataset.item));
  item.checked=input.checked;
  if(input.dataset.scope==='hero'){
    renderHero();
    $(`#hero-item-${item.id}`).focus({preventScroll:true});
    const done=heroItems.filter(i=>i.checked).length;
    notify(done===heroItems.length?'¡Todo en la cesta! La lista está completa.':`${item.name}: ${item.checked?'en la cesta':'pendiente'}. ${done} de ${heroItems.length}.`);
  }else{
    renderDemoList();
    $(`#demo-item-${item.id}`).focus({preventScroll:true});
    notify(`${item.name}: ${item.checked?'en la cesta':'pendiente'}.`);
  }
});

$('.menu-toggle').addEventListener('click',()=>{
  const opening=$('#mobile-nav').hidden;
  $('#mobile-nav').hidden=!opening;
  $('.menu-toggle').setAttribute('aria-expanded',String(opening));
  $('.menu-toggle').setAttribute('aria-label',opening?'Cerrar menú':'Abrir menú');
});
$$('#mobile-nav a').forEach(link=>link.addEventListener('click',closeMenu));
document.addEventListener('keydown',event=>{
  if(event.key==='Escape'&&!$('#mobile-nav').hidden){closeMenu();$('.menu-toggle').focus();}
});
matchMedia('(min-width:701px)').addEventListener('change',event=>{if(event.matches)closeMenu();});

$$('[data-step]').forEach((button,index)=>button.addEventListener('click',()=>setStep(index)));
$('.step-list').addEventListener('keydown',event=>{
  let index=selectedStep;
  if(event.key==='ArrowDown'||event.key==='ArrowRight')index=(index+1)%3;
  else if(event.key==='ArrowUp'||event.key==='ArrowLeft')index=(index+2)%3;
  else if(event.key==='Home')index=0;
  else if(event.key==='End')index=2;
  else return;
  event.preventDefault();setStep(index,true);
});
$$('[data-demo-tab]').forEach(button=>button.addEventListener('click',()=>setDemoTab(button.dataset.demoTab)));
$('.demo-tabs').addEventListener('keydown',event=>{
  let tab;
  if(event.key==='ArrowRight'||event.key==='ArrowLeft')tab=activeDemoTab==='list'?'expenses':'list';
  else if(event.key==='Home')tab='list';
  else if(event.key==='End')tab='expenses';
  else return;
  event.preventDefault();setDemoTab(tab,true);
});
$('#to-expenses').addEventListener('click',()=>setDemoTab('expenses',true));
$('#add-item-form').addEventListener('submit',event=>{
  event.preventDefault();const input=$('#new-item');const name=input.value.trim();
  if(!name){input.setCustomValidity('Escribe el nombre de un producto.');input.reportValidity();return;}
  demo.items.push({id:nextId++,name,quantity:'1 unidad · Sin precio de ejemplo',cents:0,checked:false});
  demo.settled=false;input.value='';renderDemoList();input.focus();notify(`${name} añadido a la lista.`);
});
$('#new-item').addEventListener('input',()=>$('#new-item').setCustomValidity(''));
$('#demo-invite').addEventListener('click',()=>{
  const panel=$('#invite-panel');panel.hidden=!panel.hidden;
  if(!panel.hidden){panel.scrollIntoView({block:'nearest',behavior:'smooth'});$('#copy-invite').focus({preventScroll:true});}
});
$('#close-invite').addEventListener('click',()=>{$('#invite-panel').hidden=true;$('#demo-invite').focus();});
$('#copy-invite').addEventListener('click',()=>{
  const range=document.createRange();range.selectNodeContents($('#invite-panel code'));
  const selection=window.getSelection();selection.removeAllRanges();selection.addRange(range);
  $('#copy-invite').textContent='Ejemplo seleccionado';
  notify('Texto seleccionado. Puedes copiarlo; no permite acceder a una lista real.');
});

function resetDemo(){
  heroItems=makeItems(seeds.casa.items);
  const scenario=seeds[demo.scenario]?demo.scenario:'casa';
  const seed=seeds[scenario];demo={scenario,title:seed.title,space:seed.space,items:makeItems(seed.items),settled:false};
  $('#new-item').value='';$('#copy-invite').textContent='Seleccionar ejemplo';$('#invite-panel').hidden=true;
  renderHero();renderDemoList();if(activeDemoTab==='expenses')renderExpenses();
  notify('Ejemplo restablecido. Puedes empezar de nuevo.');
}
$('#demo-reset').addEventListener('click',resetDemo);
$('#reset-demo').addEventListener('click',resetDemo);
$('#review-toggle').addEventListener('click',()=>{
  const panel=$('#review-panel');panel.hidden=!panel.hidden;
  $('#review-toggle').setAttribute('aria-expanded',String(!panel.hidden));
});
$('#close-review').addEventListener('click',()=>{$('#review-panel').hidden=true;$('#review-toggle').setAttribute('aria-expanded','false');$('#review-toggle').focus();});
$$('[data-width]').forEach(button=>button.addEventListener('click',()=>{
  const width=button.dataset.width;
  if(width==='full'){notify('Estás viendo la portada adaptada a tu pantalla.');return;}
  const frame=$('#responsive-frame');
  frame.style.width=`${width}px`;
  $('#viewport-title').textContent=`Vista ${width==='390'?'móvil':'tablet'} · ${width} px`;
  if(!frame.getAttribute('src')){const url=new URL(location.href);url.hash='';url.searchParams.set('embed','1');frame.src=url.href;}
  $('#review-panel').hidden=true;$('#review-toggle').setAttribute('aria-expanded','false');
  $('#viewport-dialog').showModal();
}));
$('#close-viewport').addEventListener('click',()=>$('#viewport-dialog').close());
$$('dialog').forEach(dialog=>dialog.addEventListener('click',event=>{
  if(event.target!==dialog)return;
  const rect=dialog.getBoundingClientRect();
  if(event.clientX<rect.left||event.clientX>rect.right||event.clientY<rect.top||event.clientY>rect.bottom)dialog.close();
}));
if(new URLSearchParams(location.search).get('embed')==='1')document.body.classList.add('is-frame');
renderHero();setStep(0);renderDemoList();
