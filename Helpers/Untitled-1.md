# File Tree: InmobiliariaApp-main

**Generated:** 10/31/2025, 4:26:29 PM
**Root Path:** `f:\Proyectos\InmobiliariaApp-main\InmobiliariaApp-main`

```
├── 📁 Archivos diagramas bas de datos
│   ├── 🖼️ (diagrama ER).png
│   ├── 📄 credencialesadmin.txt
│   └── 📄 mi_base_datos.sql
├── 📁 Controllers
│   ├── 📄 AuthController.cs
│   ├── 📄 ContratosApiController.cs
│   ├── 📄 ContratosController.cs
│   ├── 📄 DbSeeder.cs
│   ├── 📄 HomeController.cs
│   ├── 📄 InmueblesApiController.cs
│   ├── 📄 InmueblesController.cs
│   ├── 📄 InquilinosApiController.cs
│   ├── 📄 InquilinosController.cs
│   ├── 📄 PagosController.cs
│   ├── 📄 PerfilController.cs
│   ├── 📄 PersonasController.cs
│   ├── 📄 PropietariosApiController.cs
│   ├── 📄 PropietariosController.cs
│   ├── 📄 TiposInmueblesController.cs
│   ├── 📝 Untitled-1.md
│   ├── 📝 Untitled-2.md
│   └── 📄 UsuariosController.cs
├── 📁 Helpers
│   └── 📄 JwtHelper.cs
├── 📁 Keys
├── 📁 Models
│   ├── 📁 ViewModels
│   │   ├── 📄 CambioClaveDto.cs
│   │   ├── 📄 ContratosDetallesViewModel.cs
│   │   ├── 📄 LoginViewModel.cs
│   │   └── 📄 UsuarioRegistroViewModel.cs
│   ├── 📄 BaseModel.cs
│   ├── 📄 Contrato.cs
│   ├── 📄 ErrorViewModel.cs
│   ├── 📄 Inmueble.cs
│   ├── 📄 Inquilino.cs
│   ├── 📄 Pago.cs
│   ├── 📄 Persona.cs
│   ├── 📄 Propiedad.cs
│   ├── 📄 Propietario.cs
│   ├── 📄 RolUsuario.cs
│   ├── 📄 TipoInmueble.cs
│   └── 📄 Usuario.cs
├── 📁 Properties
│   └── ⚙️ launchSettings.json
├── 📁 Repository
│   ├── 📄 IRepoContrato.cs
│   ├── 📄 IRepoPago.cs
│   ├── 📄 IRepoTipoInmueble.cs
│   ├── 📄 IRepoUsuario.cs
│   ├── 📄 RepoContrato.cs
│   ├── 📄 RepoInmueble.cs
│   ├── 📄 RepoPago.cs
│   ├── 📄 RepoPersona.cs
│   ├── 📄 RepoTipoInmueble.cs
│   ├── 📄 RepoUsuario.cs
│   └── 📝 Untitled-2.md
├── 📁 Views
│   ├── 📁 Auth
│   │   ├── 📄 Login.cshtml
│   │   └── 📄 Register.cshtml
│   ├── 📁 Contratos
│   │   ├── 📄 Create.cshtml
│   │   ├── 📄 Delete.cshtml
│   │   ├── 📄 Details.cshtml
│   │   ├── 📄 Edit.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📁 Home
│   │   ├── 📄 Index.cshtml
│   │   └── 📄 Privacy.cshtml
│   ├── 📁 Inmuebles
│   │   ├── 📄 DisponiblesEntreFechas.cshtml
│   │   ├── 📄 Edicion.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📁 Inquilinos
│   │   ├── 📄 Create.cshtml
│   │   ├── 📄 Delete.cshtml
│   │   ├── 📄 Details.cshtml
│   │   ├── 📄 Edit.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📁 Perfil
│   │   └── 📄 Editar.cshtml
│   ├── 📁 Personas
│   │   ├── 📄 Create.cshtml
│   │   ├── 📄 Delete.cshtml
│   │   ├── 📄 Details.cshtml
│   │   ├── 📄 Edit.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📁 Propietarios
│   │   ├── 📄 Create.cshtml
│   │   ├── 📄 Delete.cshtml
│   │   ├── 📄 Details.cshtml
│   │   ├── 📄 Edit.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📁 Shared
│   │   ├── 📄 Error.cshtml
│   │   ├── 📝 Untitled-1.md
│   │   ├── 📄 _AuthLayout.cshtml
│   │   ├── 📄 _Layout.cshtml
│   │   ├── 🎨 _Layout.cshtml.css
│   │   └── 📄 _ValidationScriptsPartial.cshtml
│   ├── 📁 TiposInmuebles
│   │   ├── 📄 Create.cshtml
│   │   ├── 📄 Delete.cshtml
│   │   ├── 📄 Details.cshtml
│   │   ├── 📄 Edit.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📁 Usuarios
│   │   ├── 📄 Crear.cshtml
│   │   ├── 📄 Editar.cshtml
│   │   ├── 📄 Eliminar.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📁 pagos
│   │   ├── 📄 Create.cshtml
│   │   ├── 📄 Delete.cshtml
│   │   ├── 📄 Details.cshtml
│   │   ├── 📄 Edit.cshtml
│   │   └── 📄 Index.cshtml
│   ├── 📄 _ViewImports.cshtml
│   └── 📄 _ViewStart.cshtml
├── 📁 wwwroot
│   ├── 📁 avatars
│   │   ├── 🖼️ 0977930c-c242-4edd-ad01-31e27019c3e7.jpg
│   │   ├── 🖼️ 0f5984a6-5f23-41ba-8160-2f7608626a47.jpg
│   │   ├── 🖼️ 19d86a88-7180-4002-a941-56d3ea679b7b.jpg
│   │   ├── 🖼️ 22e70375-a081-46d7-8b29-4fa725b12e25.jpg
│   │   ├── 🖼️ 23c7064b-3041-4d52-b396-f3e7829bd77e.jpg
│   │   ├── 🖼️ 25859130-8842-4813-92c2-1926a3365af6.jpg
│   │   ├── 🖼️ 29784983-c519-4f42-af57-9c618ab48f8a.jpg
│   │   ├── 🖼️ 2bff49bd-3361-4d8a-a1ba-b95b1a04b761.jpg
│   │   ├── 🖼️ 31c211f0-4590-4c05-872b-ba9a915d27e6.jpg
│   │   ├── 🖼️ 38167a4c-05b0-41fe-8ea2-2776ba7885e9.jpg
│   │   ├── 🖼️ 398777f4-ab9f-45c2-aaea-b38a8f7da09d.jpg
│   │   ├── 🖼️ 3e41ffb2-a650-40f0-b082-7bf23fd192c1.jpg
│   │   ├── 🖼️ 43981853-6d7e-4e58-9191-0acd83b8102e.jpg
│   │   ├── 🖼️ 44128b66-f6b5-4379-9cdb-587199188337.jpg
│   │   ├── 🖼️ 528ce1fc-9e3a-472c-9c95-bb9c01677dcb.jpg
│   │   ├── 🖼️ 55169622-af87-4b78-a15f-f9e3084ed1cd.jpg
│   │   ├── 🖼️ 55216475-6726-4c52-b974-f6b54c8ec1bd.jpg
│   │   ├── 🖼️ 59f1d995-465a-4030-a32c-eeda645a1fb0.jpg
│   │   ├── 🖼️ 5c41fc15-d4e2-4b5e-bd96-6ff37397ad79.jpg
│   │   ├── 🖼️ 5c763627-54da-4ca8-a770-a27966190ae2.jpg
│   │   ├── 🖼️ 66256f56-8802-4acb-a414-246c5d18bb58.jpg
│   │   ├── 🖼️ 6d246b0b-d3cb-4c78-b10c-599381f0d588.jpg
│   │   ├── 🖼️ 7536aa49-f3a5-498c-8240-4c9a12bbcc78.jpg
│   │   ├── 🖼️ 78129238-d8e1-40a1-b6f5-4aaa71381d62.jpg
│   │   ├── 🖼️ 79d9ed9a-e3e0-4def-9bcf-fb63ec57a536.jpg
│   │   ├── 🖼️ 7b14789c-cf0c-4f66-a732-e57695d2c1ab.jpg
│   │   ├── 🖼️ 7b38a487-3a93-4853-9096-5500f11c381b.jpg
│   │   ├── 🖼️ 7d000924-3873-4d46-b58b-9dc59b90cf7c.png
│   │   ├── 🖼️ 816948a3-9e8b-49c5-9794-7dbaf690ff74.jpg
│   │   ├── 🖼️ 8694287a-cd0f-46ca-9004-bf042c2fbcb6.jpg
│   │   ├── 🖼️ 8c004e3c-ae47-4bf4-9641-73e12dfe6e3e.jpg
│   │   ├── 🖼️ 8e12493d-747f-4025-b0d4-cb80796062ae.jpg
│   │   ├── 🖼️ 9291df99-55a0-45b9-b0a5-ddec5f5a525a.jpg
│   │   ├── 🖼️ 943887e7-121a-47de-9b95-390141a0de6d.jpg
│   │   ├── 🖼️ 954c8650-9f8f-4965-830c-8c6b645a2d61.jpg
│   │   ├── 🖼️ 9773de7b-2ad5-48df-8e18-258fa991aee2.jpg
│   │   ├── 🖼️ 99f9742b-f2cf-4474-816f-0d703fd41422.jpg
│   │   ├── 🖼️ a266c9df-4145-4e07-acc8-cddf5064e993.jpg
│   │   ├── 🖼️ a50d1b21-6ea1-4006-ae5a-210a23c8ee0e.jpg
│   │   ├── 🖼️ a6f8f79f-1197-4050-b154-f7e6160815d2.jpg
│   │   ├── 🖼️ a7b55b1a-8551-4d53-af88-bfcb5a64cbb3.jpg
│   │   ├── 🖼️ a9ed8c4e-ac48-458b-84cc-50a73b79883c.jpg
│   │   ├── 🖼️ bdf21896-c38b-4f98-86f5-6b155ce2f977.jpg
│   │   ├── 🖼️ c8d24a5f-04b4-47b5-8977-b11a6c62344b.jpg
│   │   ├── 🖼️ c9143c63-29fc-4811-8c90-96ddba4e4c5c.jpg
│   │   ├── 🖼️ cae81ef2-f3c1-4810-9114-b6c62a0651fe.jpg
│   │   ├── 🖼️ d25952d6-1cec-4650-93aa-a55e72498ba3.jpg
│   │   ├── 🖼️ d6701f94-a041-4489-9d18-92c093ea284f.jpg
│   │   ├── 🖼️ d91cdc04-d1c2-4ec5-a7ce-3f9f6ca54799.jpg
│   │   ├── 🖼️ ded447e0-bc73-4ad7-9614-bc047302164f.jpg
│   │   ├── 🖼️ default.png
│   │   ├── 🖼️ e2a6e548-526a-4a7e-bb9a-798297a5d36b.jpg
│   │   ├── 🖼️ e3c1a4c0-c0ee-41da-82f6-9a0cad0aa6ac.jpg
│   │   ├── 🖼️ f0bae311-32a4-4ec5-8c91-56a602d8afe6.jpg
│   │   └── 🖼️ f4338abf-247c-482a-b106-5b50e53bb1b6.png
│   ├── 📁 css
│   │   ├── 📝 Untitled-1.md
│   │   ├── 🎨 login.css
│   │   ├── 🎨 navbar-uiverse.css
│   │   ├── 🎨 register-uiverse.css
│   │   └── 🎨 site.css
│   ├── 📁 images
│   │   └── 🖼️ casa.png
│   ├── 📁 js
│   │   └── 📄 site.js
│   ├── 📁 lib
│   │   ├── 📁 bootstrap
│   │   │   └── 📄 LICENSE
│   │   ├── 📁 jquery
│   │   │   └── 📄 LICENSE.txt
│   │   ├── 📁 jquery-validation
│   │   │   └── 📝 LICENSE.md
│   │   └── 📁 jquery-validation-unobtrusive
│   │       └── 📄 LICENSE.txt
│   ├── 📁 uploads
│   │   ├── 📁 propietarios_21
│   │   │   ├── 🖼️ 1310f5a9-31b9-46ae-a7b0-ec1623a97a63.jpg
│   │   │   ├── 🖼️ 303a5449-4ce7-4cab-baca-7e46688df512.jpg
│   │   │   ├── 🖼️ 37ca25bb-5c78-4852-9eb1-4474bf17869d.jpg
│   │   │   ├── 🖼️ 6876636c-cb18-4406-89c4-3d6186ba4b58.jpg
│   │   │   ├── 🖼️ 75e32eff-3df6-4395-969f-0080d8aa3880.jpg
│   │   │   ├── 🖼️ 7e119d35-73ea-42d8-a93f-dbeabc11b343.jpg
│   │   │   ├── 🖼️ 88cc1d98-e7bb-497a-93a2-4398b3dc969f.jpg
│   │   │   ├── 🖼️ ae650905-644d-4023-9ae0-e0e059f4967c.jpg
│   │   │   ├── 🖼️ b9cc0e44-7b5c-44af-ac95-7f56a5378a03.jpg
│   │   │   ├── 🖼️ c65d72c5-0dae-41b3-a421-3c25f6d80a43.jpg
│   │   │   └── 🖼️ d8cabace-a76b-447c-98e8-1eeb781a904b.jpg
│   │   ├── 📁 propietarios_22
│   │   │   ├── 🖼️ 34d6a33c-584d-4752-99db-5386a6611954.jpg
│   │   │   ├── 🖼️ 9591e52e-6a12-466d-9e3a-f7067191d5f7.jpg
│   │   │   ├── 🖼️ a6440251-05ae-4818-ad06-8cd57008f133.jpg
│   │   │   └── 🖼️ f42a2759-4a31-4f24-abee-ab429b35aeb2.jpg
│   │   └── 📁 propietarios_25
│   │       ├── 🖼️ 06547919-4ba0-40d8-af35-50e5fa54cdb5.jpg
│   │       ├── 🖼️ 10a7dbcf-c868-4547-9204-7b7f8173ee3b.jpg
│   │       ├── 🖼️ 2d59ef05-f1b2-49f6-aa7d-801a353207a2.jpg
│   │       ├── 🖼️ 32d698f9-e5f0-4906-8fb7-481b57ea1c22.jpg
│   │       ├── 🖼️ 3405259d-8682-4cf7-925f-69187bf404f2.jpg
│   │       ├── 🖼️ 35427027-46b7-4f6c-b9e4-6b21c17dae6c.jpg
│   │       ├── 🖼️ 3ecc4a46-c8da-4634-9d7a-5defbcac8c49.jpg
│   │       ├── 🖼️ 4405df8e-4f23-478d-8d34-26b11cdc7d67.jpg
│   │       ├── 🖼️ 4b90cf6d-248d-498e-8abe-d8a16d65ce31.jpg
│   │       ├── 🖼️ 4e98cd82-c09b-4bbd-bb8a-f8d15f3cabea.jpg
│   │       ├── 🖼️ 54411348-9151-4f7c-b761-408f4fc14cb3.jpg
│   │       ├── 🖼️ 59de8c5c-d79a-46bc-ad7e-613eb71c5093.jpg
│   │       ├── 🖼️ 5c542a69-922e-466f-9045-c64c1ae95b3a.jpg
│   │       ├── 🖼️ 758cd80c-359a-42d3-911d-fddda3ec1ab8.jpg
│   │       ├── 🖼️ 839874ef-f22a-40c3-8b1a-66cd55e9a190.jpg
│   │       ├── 🖼️ 8a100a00-7564-4ca1-8c79-411942e22384.jpg
│   │       ├── 🖼️ 8a12fdd3-3958-4d63-84ec-4a141b1686e3.jpg
│   │       ├── 🖼️ a454d79e-9a8e-4498-a066-c7741421dfcd.jpg
│   │       ├── 🖼️ a8078d82-b7de-4bf6-8e13-a86ea9ad53d4.jpg
│   │       ├── 🖼️ a85b622c-9de6-4e33-b50a-f5ba07970498.jpg
│   │       ├── 🖼️ ad17969c-d306-45bf-9ea7-ff72636ac410.jpg
│   │       ├── 🖼️ bc960372-4e96-4573-be51-0dbd523defcf.jpg
│   │       ├── 🖼️ c0ae3ecb-23c4-4c09-9d22-ecab683967d0.jpg
│   │       ├── 🖼️ cf8b05fa-81c8-4aa9-bfbf-e4d63aa2bbb8.jpg
│   │       ├── 🖼️ dc180433-c4a7-4077-b8b2-528a646b1d42.jpg
│   │       ├── 🖼️ eade9051-b865-4bd4-8b4a-aa1382e88788.jpg
│   │       ├── 🖼️ f0d0e948-00b7-4fa1-bcf5-25445d06bd22.jpg
│   │       ├── 🖼️ f36d5d07-81ec-4bc7-a9e5-30e30f2929ea.jpg
│   │       └── 🖼️ ff6133f9-83ef-448a-9353-9bc7b0062fd1.jpg
│   └── 📄 favicon.ico
├── ⚙️ .gitignore
├── 📄 InmobiliariaApp.csproj
├── 📄 InmobiliariaApp.sln
├── 📄 Program.cs
├── 📝 Untitled-1.md
├── ⚙️ appsettings.json
└── ⚙️ test.json
```

---
*Generated by FileTree Pro Extension*