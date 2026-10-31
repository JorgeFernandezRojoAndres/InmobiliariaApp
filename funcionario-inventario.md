# InmobiliariaApp — Inventario de funciones (para revisión)

> **Leyenda:** ~~tachado~~ ✅ = funcionalidad probada en sesión (app o BD real ejecutándose).
> Lo que no está tachado sigue **sin verificar**.

## 1. CONTROLLERS (módulo web MVC)

### 1.1 AuthController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Register()` | GET | `/Auth/Register` | `AllowAnonymous` | ~~Correcta, vista normal.~~ ✅ **Probado**: el formulario abre vacío, sin datos de prueba. |
| `Register(UsuarioRegistroViewModel, IFormFile?)` | POST | `/Auth/Register` | `AllowAnonymous` | ~~Correcta: hash de contraseña, avatar opcional, redirige a Login.~~ ✅ **Probado**: usuario nuevo creado con hash BCrypt y aislado en su propia inmobiliaria (email único validado). |
| `Login()` | GET | `/Auth/Login` | `AllowAnonymous` | ~~Correcta, vista normal.~~ ✅ **Probado**: 200 y token antiforgery emitido. |
| `Login(LoginViewModel)` | POST | `/Auth/Login` | `AllowAnonymous` | ~~Correcta: verifica BCrypt, genera claims, firma sesión cookie, redirige a Home.~~ ✅ **Probado E2E local**: `302 → /Home/Index` con cookie de sesión (admin y usuario nuevo). |
| `Logout()` | GET | `/Auth/Logout` | `Authorize` | Correcta: cierra sesión y redirige a Login. |

### 1.2 ContratosController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index(inicio, fin, propietarioId)` | GET | `/Contratos` | `Propietario, Administrador, Empleado` | Coincide con la barra `asp-controller="Contratos"` / `asp-action="Index"`. |
| `Renovar(id)` | GET | `/Contratos/Renovar/{id}` | Inferido del controller | Correcta, pero solo redirige a Create; no aplica lógica de renovación. |
| `PorInmueble(id)` | GET | `/Contratos/PorInmueble/{id}` | Inferido | Correcta. |
| `Details(id)` | GET | `/Contratos/Details/{id}` | Inferido | Correcta. |
| `Create()` | GET | `/Contratos/Create` | Inferido | Correcta. |
| `Create(Contrato)` | POST | `/Contratos/Create` | Inferido | Correcta, recibe los datos del formulario y guarda. |
| `Edit(id)` | GET | `/Contratos/Edit/{id}` | Inferido | Correcta. |
| `Edit(id, Contrato)` | POST | `/Contratos/Edit/{id}` | Inferido | Valida `Estado` contra los valores permitidos. |
| `Delete(id)` | GET | `/Contratos/Delete/{id}` | Inferido | Correcta. |
| `DeleteConfirmed(id)` | POST | `/Contratos/Delete/{id}` | Inferido | Correcta, usa `ActionName("Delete")`. |
| `VigentesEntreFechas(inicio, fin)` | GET | `/Contratos/VigentesEntreFechas` | Inferido | ~~Correcta.~~ ✅ **Probado**: query `ObtenerVigentesEntre` (con `NOT EXISTS`) corrió contra la BD real sin errores. |
| `Rescindir(id)` | POST | `/Contratos/Rescindir/{id}` | Inferido | Correcta. |

### 1.3 UsuariosController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index()` | GET | `/Usuarios` | `Administrador` | Correcta. |
| `Crear()` | GET | `/Usuarios/Crear` | `Administrador` | Correcta. |
| `Crear(Usuario, IFormFile?)` | POST | `/Usuarios/Crear` | `Administrador` | Correcta. |
| `Editar(id)` | GET | `/Usuarios/Editar/{id}` | `Administrador` | Correcta. |
| `Editar(id, Usuario, ...)` | POST | `/Usuarios/Editar/{id}` | `Administrador` | Correcta. |
| `Eliminar(id)` | GET | `/Usuarios/Eliminar/{id}` | `Administrador` | Correcta. |
| `EliminarConfirmado(id)` | POST | `/Usuarios/Eliminar/{id}` | `Administrador` | Correcta. |

### 1.4 PagosController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index()` | GET | `/Pagos` | `Administrador` | Correcta. |
| `Details(id)` | GET | `/Pagos/Details/{id}` | `Administrador` | Correcta. |
| `Create()` | GET | `/Pagos/Create` | `Administrador` | Correcta. |
| `Create(Pago)` | POST | `/Pagos/Create` | `Administrador` | Correcta, envía notificación Firebase. |
| `Edit(id)` | GET | `/Pagos/Edit/{id}` | `Administrador` | Correcta. |
| `Edit(id, Pago)` | POST | `/Pagos/Edit/{id}` | `Administrador` | Correcta. |
| `Delete(id)` | GET | `/Pagos/Delete/{id}` | `Administrador` | Correcta. |
| `DeleteConfirmed(id)` | POST | `/Pagos/Delete/{id}` | `Administrador` | Correcta. |

### 1.5 InmueblesController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index()` | GET | `/Inmuebles` | `Authorize` | Correcta: listado. |
| `Disponibles()` | GET | `/Inmuebles/Disponibles` | `Authorize` | ~~Correcta.~~ ✅ **Probado**: query `ObtenerDisponibles` (con `NOT EXISTS`) verificada contra BD real. |
| `PorPropietario(id)` | GET | `/Inmuebles/PorPropietario/{id}` | `Authorize` | Correcta. |
| `ActivosPorPropietario(propietarioid)` | GET | `/Inmuebles/ActivosPorPropietario?propietarioid=` | `Authorize` | Correcta. |
| `InactivosPorPropietario(propietarioid)` | GET | `/Inmuebles/InactivosPorPropietario?propietarioid=` | `Authorize` | Correcta. |
| `DisponiblesEntreFechas(inicio, fin)` | GET | `/Inmuebles/DisponiblesEntreFechas` | `Authorize` | ~~Correcta.~~ ✅ **Probado tras fix**: 26 filas (antes daba 500 por un `)` faltante en `NOT EXISTS`); corregido y desplegado. |
| `Edicion(id = 0)` | GET | `/Inmuebles/Edicion/{id}` | `Authorize` | Correcta. |
| `BuscarPorPropietario(propietarioId)` | GET | `/Inmuebles/BuscarPorPropietario/{propietarioId}` | `Authorize` | Correcta. |
| `Guardar(Inmueble)` | POST | `/Inmuebles/Guardar` | `Authorize` | Correcta. |
| `Eliminar(id)` | POST | `/Inmuebles/Eliminar/{id}` | `Authorize` | Correcta, borrado lógico. |

### 1.6 InquilinosController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index()` | GET | `/Inquilinos` | `Authorize` | Correcta. |
| `Details(id)` | GET | `/Inquilinos/Details/{id}` | `Authorize` | Correcta. |
| `Create()` | GET | `/Inquilinos/Create` | `Authorize` | Correcta. |
| `Create(Persona)` | POST | `/Inquilinos/Create` | `Authorize` | Correcta, asigna rol `Inquilino`. |
| `Edit(id)` | GET | `/Inquilinos/Edit/{id}` | `Authorize` | Correcta. |
| `Edit(id, Persona)` | POST | `/Inquilinos/Edit/{id}` | `Authorize` | Correcta. |
| `Delete(id)` | GET | `/Inquilinos/Delete/{id}` | `Authorize` | Correcta. |
| `DeleteConfirmed(id)` | POST | `/Inquilinos/Delete/{id}` | `Authorize` | Correcta. |

### 1.7 PersonasController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index()` | GET | `/Personas` | `Authorize` | ~~Correcta.~~ ✅ **Probado**: columna *Tipo* ahora muestra `Propietario` / `Inquilino` (query con `GROUP_CONCAT` verificada contra BD real). |
| `Details(id)` | GET | `/Personas/Details/{id}` | `Authorize` | Correcta. |
| `Create()` | GET | `/Personas/Create` | `Authorize` | Correcta. |
| `Create(Persona, bool, bool)` | POST | `/Personas/Create` | `Authorize` | Correcta, asigna roles `Propietario` y/o `Inquilino`. |
| `Edit(id)` | GET | `/Personas/Edit/{id}` | `Authorize` | Correcta. |
| `Edit(id, Persona)` | POST | `/Personas/Edit/{id}` | `Authorize` | Correcta. |
| `Delete(id)` | GET | `/Personas/Delete/{id}` | `Authorize` | Correcta. |
| `DeleteConfirmed(id)` | POST | `/Personas/Delete/{id}` | `Authorize` | Correcta. |

### 1.8 PropietariosController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index()` | GET | `/Propietarios` | `Cookie` | Correcta. |
| `Details(id)` | GET | `/Propietarios/Details/{id}` | `Cookie` | Correcta. |
| `Create()` | GET | `/Propietarios/Create` | `Cookie` | Correcta. |
| `Create(Persona)` | POST | `/Propietarios/Create` | `Cookie` | Correcta, asigna rol `Propietario` y redirige a Inmuebles/Create. |
| `Edit(id)` | GET | `/Propietarios/Edit/{id}` | `Cookie` | Correcta. |
| `Edit(id, Persona)` | POST | `/Propietarios/Edit/{id}` | `Cookie` | Correcta. |
| `Delete(id)` | GET | `/Propietarios/Delete/{id}` | `Cookie` | Correcta. |
| `DeleteConfirmed(id)` | POST | `/Propietarios/Delete/{id}` | `Cookie` | Correcta. |

### 1.9 PerfilController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Editar()` | GET | `/Perfil/Editar` | `Authorize` | ~~Correcta.~~ ✅ **Probado E2E local**: `200` con sesión cookie. |
| `Editar(Usuario, ..., IFormFile?, ...)` | POST | `/Perfil/Editar` | `Authorize` | ~~Correcta, actualiza datos, avatar, contraseña.~~ ✅ **Probado E2E local**: fotos de 70 B y 5 MB → `302` y archivo escrito en `wwwroot/avatars`, sin excepciones. ⚠️ El `ERR_FAILED` en producción es del proxy TLS, no de esta acción. |

### 1.10 HomeController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Index()` | GET | `/` | `Authorize` | Correcta. |
| `MarcarFavorita(id)` | GET | `/Home/MarcarFavorita` | `Authorize` | Correcta. |
| `BorrarFavorita()` | POST | `/Home/BorrarFavorita` | `Authorize` | Correcta. |
| `Privacy()` | GET | `/Home/Privacy` | `Authorize` | Correcta, evita 404 en el menú. |

## 2. CONTROLLERS API (módulo mobile)

### 2.1 ContratosApiController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `GetVigentes()` | GET | `/api/Contratos/vigentes` | JWT | Correcta. |
| `GetFinalizados()` | GET | `/api/Contratos/finalizados` | JWT | Correcta. |
| `GetTodos()` | GET | `/api/Contratos/todos` | JWT | Correcta. |
| `GetPagosPorContrato(id)` | GET | `/api/Contratos/{id}/pagos` | JWT | Correcta. |
| `RenovarContr
