//******************************PORTUGUÊS******************************//

# LogisControl-Backend - PDS-IPCA

API RESTful desenvolvida em ASP.NET Core para suportar a plataforma LogisControl, dedicada à gestão integrada de processos industriais. Inclui módulos de produção, manutenção, compras, armazém e gestão de utilizadores, com autenticação JWT e acesso a dados via SQL Server.

## Funcionalidades principais

* Gestão de utilizadores com autenticação JWT e controlo de permissões.
* Gestão de produtos, matérias-primas, clientes, fornecedores e máquinas.
* Módulos completos de:
  * Compras (pedidos, cotações, encomendas)
  * Produção (ordens, registos, consumos)
  * Manutenção (tickets, assistência, registos)
* Notificações por email e Telegram (stock crítico, pedidos, alertas).
* Arquitetura em camadas (Models, DTOs, Services, Interfaces, Controllers).
* Persistência de dados através de Entity Framework Core e SQL Server.
* Documentação via Swagger/OpenAPI.

## Conexão ao Frontend
O frontend correspondente encontra-se no repositório **LogisControl-Frontend-PDS-IPCA**, comunicando com esta API através de endpoints REST.

//******************************ENGLISH******************************//

# LogisControl-Backend - PDS-IPCA

RESTful API built with ASP.NET Core to power the LogisControl industrial management platform. Provides integrated modules for production, maintenance, procurement, warehouse operations, and user management, featuring JWT authentication and SQL Server persistence.

## Main Features

* User management with JWT authentication and role-based access control.
* Management of products, raw materials, clients, suppliers, and machines.
* Complete modules for:
  * Procurement (requests, quotations, purchase orders)
  * Production (work orders, production logs, material consumption)
  * Maintenance (tickets, assistance, maintenance records)
* Email and Telegram notifications (critical stock, requests, alerts).
* Layered architecture (Models, DTOs, Services, Interfaces, Controllers).
* Data persistence using Entity Framework Core and SQL Server.
* Documentation provided through Swagger/OpenAPI.

## Frontend Connection
The corresponding frontend is available in the **LogisControl-Frontend-PDS-IPCA** repository and communicates with this API through REST endpoints.
