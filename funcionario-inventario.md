# InmobiliariaApp — Inventario de funciones (para revisión)

## 1. CONTROLLERS (módulo web MVC)

### 1.1 AuthController
| Acción | Tipo | Ruta | Autorización | Estado observado |
|---|---|---|---|---|
| `Register()` | GET | `/Auth/Register` | `AllowAnonymous` | Correcta, vista normal. |
| `Register(UsuarioRegistroViewModel, IFormFile?)` | POST | `/Auth/Register` | `AllowAnonymous` | Correcta: hash de contraseña, avatar opcional, redirige a Login. |
| `Login()` | GET | `/Auth/Login` | `AllowAnonymous` | Correcta, vista normal. |
| `Login(LoginViewModel)` | POST | `/Auth/Login` | `AllowAnonymous` | Correcta: verifica BCrypt, genera claims, firma sesión cookie, redirige a Home. |
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
| `VigentesEntreFechas(inicio, fin)` | GET | `/Contratos/VigentesEntreFechas` | Inferido | Correcta. |
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
| `Disponibles()` | GET | `/Inmuebles/Disponibles` | `Authorize` | Correcta. |
| `PorPropietario(id)` | GET | `/Inmuebles/PorPropietario/{id}` | `Authorize` | Correcta. |
| `ActivosPorPropietario(propietarioid)` | GET | `/Inmuebles/ActivosPorPropietario?propietarioid=` | `Authorize` | Correcta. |
| `InactivosPorPropietario(propietarioid)` | GET | `/Inmuebles/InactivosPorPropietario?propietarioid=` | `Authorize` | Correcta. |
| `DisponiblesEntreFechas(inicio, fin)` | GET | `/Inmuebles/DisponiblesEntreFechas` | `Authorize` | Correcta. |
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
| `Index()` | GET | `/Personas` | `Authorize` | Correcta. |
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
| `Editar()` | GET | `/Perfil/Editar` | `Authorize` | Correcta. |
| `Editar(Usuario, ..., IFormFile?, ...)` | POST | `/Perfil/Editar` | `Authorize` | Correcta, actualiza datos, avatar, contraseña. |

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
