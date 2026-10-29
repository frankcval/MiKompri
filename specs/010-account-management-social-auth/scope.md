# 010 — Account Management & Social Auth (solo alcance)

Sin implementación. UserId GUID interno inmutable; Users posee correlación y perfil. Evaluar Entra External ID CIAM, Google/otros y registro email (preferentemente en IdP, no contraseñas propias en Users), migración/federación de Entra actual preservando ownership/membresías/historia. Linking explícito con prueba reciente de ambas identidades; nunca por coincidencia email. Desvinculación/recuperación sin perder último método.

Separar claims externos y datos propios: username/display name, nombre/apellidos, avatar propio/proveedor, nacimiento opcional, email/verificación/procedencia, teléfono/verificación, dirección opcional, preferencias. No sobrescribir personalizaciones al sincronizar claims. Relación varias identidades externas → UserId. ADR y revisión TP11 antes de cambiar proveedor/tenant/correlación; tokens/audience/scopes, Web y MAUI, migración y rollback probados.

Referencia investigada: https://learn.microsoft.com/en-us/entra/external-id/customers/concept-authentication-methods-customers . External ID ofrece email/password u OTP y proveedores sociales; social requiere autenticación delegada navegador. Revalidar disponibilidad/configuración al formalizar 010.
