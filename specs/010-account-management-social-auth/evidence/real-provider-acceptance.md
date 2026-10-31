# Proveedores reales — aceptación parcial 010-A

31/10/2026; [validación técnica](validation.md), [recuperación actual](authentication-recovery-2026-10-31.md), [traslado](migration-rehearsal.md). Web/APIs normales en QA aislado; no mocks en estos recorridos. No es aceptación de producción.

| Recorrido | Observación | Límite |
|---|---|---|
| Google sin Microsoft,30/10 | Cuenta QA previamente confirmada por titular sin Microsoft; Users/me, ShoppingList/mine, catálogo y mercados200, UserId propio distinto de Microsoft | Antes de corrección de hint; datos QA tmpfs anteriores perdidos al reiniciar |
| Microsoft legacy,30/10 | Después de logout Google, APIs normales200; Guid igual a la semilla canónica Microsoft, perfil Google ausente | No demuestra Microsoft federado/traslado ni todos los escenarios multi-tab |
| Perfil Google,30/10 | PUT200 y sync200 conservaron personalización/clear | No se completó logout→login Google después de editar |
| Acceso del titular,31/10 | Tras retirar hint llega a listas y edita perfil; informa de OTP por acceso y confirma pulsar Google/elegir cuenta. Graph de la fila examinada hasta entonces muestra issuer mail | No se atribuye su último desafío a correo o Google sin identificar el dominio/petición; tampoco acredita T056 |
| Google,31/10 | Navegador de pruebas: Usar otra cuenta→Iniciar sesión con Google→SSO→localhost/lists sin OTP; nueva fila Users, Graph acredita issuer google.com | No demuestra negativo de token Google directo ni traslado Microsoft |
| Nombre visible,31/10 | Titular ve unknown; lectura QA Graph y Users confirma valor inicial unknown sin nombres/apellidos, versión0 | No se modifica su perfil mediante herramientas. Pendiente editar/guardar/relogin mediante UI |

El titular confirma guardar sus cambios; versión de perfil aumenta. Falta atribuir y completar ese relogin, por lo que T050 Google sigue pendiente. El objeto Google QA también trae unknown; el flujo observado en portal solo recopila Email, sin Display Name/Given Name/Surname. No se ha modificado ese flujo durante el diagnóstico ni se presupone que la configuración futura rellene perfiles anteriores. Inicialización desde atributos Google en alta nueva sigue pendiente.

No se publican correos, nombres personales, tuplas/Guids de consumidores, tokens ni códigos. El GUID se comparó privadamente en el recorrido30/10, sin adoptar por email. Mantener Microsoft legacy es requisito de continuidad, no prueba de traslado cross-tenant.

Pendiente: Microsoft actual y cambio de cuenta en el candidato corregido; personalización Google con logout/login; negativo real de token Google contra tres APIs normales; positivo D1 tras corte; traslado Microsoft y rollback con manifiestos. T047/T050/T051/T048 siguen abiertos. Las pruebas del harness anterior conservan su valor parcial y no sustituyen estas comprobaciones.
