--CRIAR A BASE DE DADOS NA MASTER CORRER SO ESTES DOIS
CREATE DATABASE LogisControl
GO

--SAIR DA MASTER E COLOCAR A NOVA E CORRER TUDO ATE LA BAIXO PELA ORDEM QUE ESTA
USE LogisControl;
GO

------------------------------------------------------------------------------------

-- Criar o login no servidor
CREATE LOGIN bd WITH PASSWORD = 'bd';

-- Atribuir permissões ao utilizador na base de dados
USE LogisControl;
CREATE USER bd FOR LOGIN bd;

-- Dar permissões básicas (leitura e escrita)
ALTER ROLE db_datareader ADD MEMBER bd;
ALTER ROLE db_datawriter ADD MEMBER bd;

-- Permitir que o utilizador se conecte à base
GRANT CONNECT TO bd;

----------------------------------------------------------------------------------------

CREATE TABLE Utilizador (
  UtilizadorID   int IDENTITY NOT NULL, 
  PrimeiroNome   varchar(25) NOT NULL, 
  Sobrenome      varchar(25) NOT NULL, 
  NumFuncionario int NOT NULL UNIQUE, 
  Password       varchar(25) NOT NULL, 
  Role           varchar(15) NOT NULL, 
  Estado         bit NOT NULL, 
  PRIMARY KEY (UtilizadorID));
CREATE TABLE MateriaPrima (
  MateriaPrimalD int IDENTITY NOT NULL, 
  Nome           varchar(100) NOT NULL, 
  Quantidade     int NOT NULL, 
  Descricao      varchar(1000) NOT NULL, 
  Categoria      varchar(25) NOT NULL, 
  CodInterno     varchar(1000) NOT NULL UNIQUE, 
  Preco          float(53) NOT NULL, 
  PRIMARY KEY (MateriaPrimalD));
CREATE TABLE Produto (
  ProdutoID                    int IDENTITY NOT NULL, 
  Nome                         varchar(100) NOT NULL, 
  Quantidade                   varchar(1000) NOT NULL, 
  Descricao                    varchar(1000) NOT NULL, 
  CodInterno                   varchar(1000) NOT NULL, 
  Preco                        float(53) NOT NULL, 
  OrdemProducaoOrdemProdID     int NOT NULL, 
  EncomendaItensEncomendaItens int NOT NULL, 
  PRIMARY KEY (ProdutoID));
CREATE TABLE Maquina (
  MaquinaID                      int IDENTITY NOT NULL, 
  Nome                           varchar(100) NOT NULL, 
  LinhaProd                      int NOT NULL, 
  AssistenciaExternaAssistenteID int NOT NULL, 
  PRIMARY KEY (MaquinaID));
CREATE TABLE AssistenciaExterna (
  AssistenteID int IDENTITY NOT NULL, 
  Nome         varchar(100) NOT NULL, 
  NIF          int NOT NULL, 
  Morada       varchar(100) NOT NULL, 
  Telefone     int NOT NULL, 
  PRIMARY KEY (AssistenteID));
CREATE TABLE PedidoManutencao (
  PedidoManutID          int IDENTITY NOT NULL, 
  Descicao               varchar(1000) NOT NULL, 
  Estado                 varchar(10) NOT NULL, 
  DataAbertura           datetime NOT NULL, 
  DataConclusao          datetime NULL, 
  MaquinaMaquinaID       int NOT NULL, 
  UtilizadorUtilizadorID int NOT NULL, 
  PRIMARY KEY (PedidoManutID));
CREATE TABLE EncomendaCliente (
  EncomendaClienteID int IDENTITY NOT NULL, 
  DataEncomenda      datetime NOT NULL, 
  Estado             varchar(10) NOT NULL, 
  ClienteClienteID   int NOT NULL, 
  PRIMARY KEY (EncomendaClienteID));
CREATE TABLE Cliente (
  ClienteID int IDENTITY NOT NULL, 
  Nome      varchar(100) NOT NULL, 
  NIF       int NOT NULL, 
  Morada    varchar(100) NOT NULL, 
  PRIMARY KEY (ClienteID));
CREATE TABLE OrdemProducao (
  OrdemProdID                        int IDENTITY NOT NULL, 
  Estado                             varchar(10) NOT NULL, 
  Quantidade                         int NOT NULL, 
  DataAbertura                       datetime NOT NULL, 
  DataConclusao                      datetime NULL, 
  MaquinaMaquinaID                   int NOT NULL, 
  EncomendaClienteEncomendaClienteID int NOT NULL, 
  PRIMARY KEY (OrdemProdID));
CREATE TABLE ProdMateriais (
  ProdMateriaisID            int IDENTITY NOT NULL, 
  QuantidadeUtilizada        int NOT NULL, 
  OrdemProducaoOrdemProdID   int NOT NULL, 
  MateriaPrimaMateriaPrimalD int NOT NULL, 
  PRIMARY KEY (ProdMateriaisID));
CREATE TABLE Fornecedor (
  FornecedorID int IDENTITY NOT NULL, 
  Nome         varchar(100) NOT NULL, 
  Telefone     int NULL, 
  Email        varchar(25) NULL, 
  PRIMARY KEY (FornecedorID));
CREATE TABLE Orcamento (
  OrcamentoID                  int IDENTITY NOT NULL, 
  Data                         datetime NOT NULL, 
  Estado                       varchar(10) NOT NULL, 
  PedidoCotacaoPedidoCotacaoID int NOT NULL, 
  PRIMARY KEY (OrcamentoID));
CREATE TABLE PedidoCompra (
  PedidoCompraID         int IDENTITY NOT NULL, 
  Descricao              varchar(1000) NOT NULL, 
  Estado                 varchar(10) NOT NULL, 
  DataAbertura           datetime NOT NULL, 
  DataConclusao          datetime NULL, 
  UtilizadorUtilizadorID int NOT NULL, 
  PRIMARY KEY (PedidoCompraID));
CREATE TABLE MateriaPrimaProduto (
  MateriaPrimaProdutoID      int IDENTITY NOT NULL, 
  QuantidadeNec              int NOT NULL, 
  MateriaPrimaMateriaPrimalD int NOT NULL, 
  ProdutoProdutoID           int NOT NULL, 
  PRIMARY KEY (MateriaPrimaProdutoID));
CREATE TABLE OrcamentoItem (
  OrcamentoItemID            int IDENTITY NOT NULL, 
  Quantidade                 int NOT NULL, 
  PrecoUnit                  float(53) NOT NULL, 
  PrazoEntrega               int NULL, 
  OrcamentoOrcamentoID       int NOT NULL, 
  MateriaPrimaMateriaPrimalD int NOT NULL, 
  PRIMARY KEY (OrcamentoItemID));
CREATE TABLE NotaEncomenda (
  NotaEncomendaID      int IDENTITY NOT NULL, 
  DataEmissao          datetime NOT NULL, 
  Estado               varchar(10) NOT NULL, 
  ValorTotal           float(53) NOT NULL, 
  OrcamentoOrcamentoID int NOT NULL, 
  PRIMARY KEY (NotaEncomendaID));
CREATE TABLE NotaEncomendaItens (
  NotaEncomendaItensID         int IDENTITY NOT NULL, 
  Quantidade                   int NOT NULL, 
  PrecoUnit                    float(53) NOT NULL, 
  NotaEncomendaNotaEncomendaID int NOT NULL, 
  MateriaPrimaMateriaPrimalD   int NOT NULL, 
  PRIMARY KEY (NotaEncomendaItensID));
CREATE TABLE RegistoManutencao (
  RegistoManutencaoID            int IDENTITY NOT NULL, 
  Descricao                      varchar(1000) NOT NULL, 
  Estado                         varchar(10) NOT NULL, 
  PedidoManutencaoPedidoManutID  int NOT NULL, 
  UtilizadorUtilizadorID         int NOT NULL, 
  AssistenciaExternaAssistenteID int NOT NULL, 
  PRIMARY KEY (RegistoManutencaoID));
CREATE TABLE EncomendaItens (
  EncomendaItens                     int IDENTITY NOT NULL, 
  Quantidade                         int NULL, 
  EncomendaClienteEncomendaClienteID int NOT NULL, 
  PRIMARY KEY (EncomendaItens));
CREATE TABLE PedidoCotacao (
  PedidoCotacaoID        int IDENTITY NOT NULL, 
  Descricao              varchar(1000) NOT NULL, 
  Data                   datetime NOT NULL, 
  Estado                 varchar(10) NULL, 
  FornecedorFornecedorID int NOT NULL, 
  PRIMARY KEY (PedidoCotacaoID));
CREATE TABLE RegistoProducao (
  RegistoProducaoID        int IDENTITY NOT NULL, 
  Estado                   varchar(10) NOT NULL, 
  DataProducao             datetime NOT NULL, 
  Observacoes              varchar(100) NULL, 
  UtilizadorUtilizadorID   int NOT NULL, 
  ProdutoProdutoID         int NOT NULL, 
  OrdemProducaoOrdemProdID int NOT NULL, 
  PRIMARY KEY (RegistoProducaoID));
ALTER TABLE MateriaPrimaProduto ADD CONSTRAINT FKMateriaPri691498 FOREIGN KEY (MateriaPrimaMateriaPrimalD) REFERENCES MateriaPrima (MateriaPrimalD);
ALTER TABLE MateriaPrimaProduto ADD CONSTRAINT FKMateriaPri932813 FOREIGN KEY (ProdutoProdutoID) REFERENCES Produto (ProdutoID);
ALTER TABLE OrcamentoItem ADD CONSTRAINT FKOrcamentoI811131 FOREIGN KEY (OrcamentoOrcamentoID) REFERENCES Orcamento (OrcamentoID);
ALTER TABLE OrcamentoItem ADD CONSTRAINT FKOrcamentoI247272 FOREIGN KEY (MateriaPrimaMateriaPrimalD) REFERENCES MateriaPrima (MateriaPrimalD);
ALTER TABLE NotaEncomenda ADD CONSTRAINT FKNotaEncome702864 FOREIGN KEY (OrcamentoOrcamentoID) REFERENCES Orcamento (OrcamentoID);
ALTER TABLE NotaEncomendaItens ADD CONSTRAINT FKNotaEncome419194 FOREIGN KEY (NotaEncomendaNotaEncomendaID) REFERENCES NotaEncomenda (NotaEncomendaID);
ALTER TABLE NotaEncomendaItens ADD CONSTRAINT FKNotaEncome850921 FOREIGN KEY (MateriaPrimaMateriaPrimalD) REFERENCES MateriaPrima (MateriaPrimalD);
ALTER TABLE PedidoCompra ADD CONSTRAINT FKPedidoComp502741 FOREIGN KEY (UtilizadorUtilizadorID) REFERENCES Utilizador (UtilizadorID);
ALTER TABLE PedidoManutencao ADD CONSTRAINT FKPedidoManu589131 FOREIGN KEY (MaquinaMaquinaID) REFERENCES Maquina (MaquinaID);
ALTER TABLE PedidoManutencao ADD CONSTRAINT FKPedidoManu67204 FOREIGN KEY (UtilizadorUtilizadorID) REFERENCES Utilizador (UtilizadorID);
ALTER TABLE RegistoManutencao ADD CONSTRAINT FKRegistoMan791617 FOREIGN KEY (PedidoManutencaoPedidoManutID) REFERENCES PedidoManutencao (PedidoManutID);
ALTER TABLE RegistoManutencao ADD CONSTRAINT FKRegistoMan601844 FOREIGN KEY (UtilizadorUtilizadorID) REFERENCES Utilizador (UtilizadorID);
ALTER TABLE RegistoManutencao ADD CONSTRAINT FKRegistoMan998094 FOREIGN KEY (AssistenciaExternaAssistenteID) REFERENCES AssistenciaExterna (AssistenteID);
ALTER TABLE Maquina ADD CONSTRAINT FKMaquina431467 FOREIGN KEY (AssistenciaExternaAssistenteID) REFERENCES AssistenciaExterna (AssistenteID);
ALTER TABLE Produto ADD CONSTRAINT FKProduto647431 FOREIGN KEY (OrdemProducaoOrdemProdID) REFERENCES OrdemProducao (OrdemProdID);
ALTER TABLE ProdMateriais ADD CONSTRAINT FKProdMateri73484 FOREIGN KEY (OrdemProducaoOrdemProdID) REFERENCES OrdemProducao (OrdemProdID);
ALTER TABLE ProdMateriais ADD CONSTRAINT FKProdMateri754466 FOREIGN KEY (MateriaPrimaMateriaPrimalD) REFERENCES MateriaPrima (MateriaPrimalD);
ALTER TABLE OrdemProducao ADD CONSTRAINT FKOrdemProdu935284 FOREIGN KEY (MaquinaMaquinaID) REFERENCES Maquina (MaquinaID);
ALTER TABLE EncomendaCliente ADD CONSTRAINT FKEncomendaC556375 FOREIGN KEY (ClienteClienteID) REFERENCES Cliente (ClienteID);
ALTER TABLE OrdemProducao ADD CONSTRAINT FKOrdemProdu255309 FOREIGN KEY (EncomendaClienteEncomendaClienteID) REFERENCES EncomendaCliente (EncomendaClienteID);
ALTER TABLE Produto ADD CONSTRAINT FKProduto171408 FOREIGN KEY (EncomendaItensEncomendaItens) REFERENCES EncomendaItens (EncomendaItens);
ALTER TABLE EncomendaItens ADD CONSTRAINT FKEncomendaI428182 FOREIGN KEY (EncomendaClienteEncomendaClienteID) REFERENCES EncomendaCliente (EncomendaClienteID);
ALTER TABLE Orcamento ADD CONSTRAINT FKOrcamento236624 FOREIGN KEY (PedidoCotacaoPedidoCotacaoID) REFERENCES PedidoCotacao (PedidoCotacaoID);
ALTER TABLE RegistoProducao ADD CONSTRAINT FKRegistoPro479439 FOREIGN KEY (UtilizadorUtilizadorID) REFERENCES Utilizador (UtilizadorID);
ALTER TABLE RegistoProducao ADD CONSTRAINT FKRegistoPro11086 FOREIGN KEY (ProdutoProdutoID) REFERENCES Produto (ProdutoID);
ALTER TABLE RegistoProducao ADD CONSTRAINT FKRegistoPro913410 FOREIGN KEY (OrdemProducaoOrdemProdID) REFERENCES OrdemProducao (OrdemProdID);
ALTER TABLE PedidoCotacao ADD CONSTRAINT FKPedidoCota347557 FOREIGN KEY (FornecedorFornecedorID) REFERENCES Fornecedor (FornecedorID);



-- Inserir Utilizadores
INSERT INTO Utilizador (PrimeiroNome, Sobrenome, NumFuncionario, Password, Role, Estado) VALUES
('João', 'Silva', 1001, 'pass123', 'Operador', 1),
('Maria', 'Ferreira', 1002, 'pass456', 'Gestor', 1),
('Carlos', 'Santos', 1003, 'pass789', 'Técnico', 1),
('Ana', 'Costa', 1004, 'pass321', 'Supervisor', 1),
('Bruno', 'Oliveira', 1005, 'pass654', 'Operador', 1);

-- Inserir Fornecedores
INSERT INTO Fornecedor (Nome, Telefone, Email) VALUES
('Fornecedor A', 123456789, 'fornecedorA@email.com'),
('Fornecedor B', 987654321, 'fornecedorB@email.com'),
('Fornecedor C', 112233445, 'fornecedorC@email.com'),
('Fornecedor D', 556677889, 'fornecedorD@email.com'),
('Fornecedor E', 667788990, 'fornecedorE@email.com');

-- Inserir Clientes
INSERT INTO Cliente (Nome, NIF, Morada) VALUES
('Empresa A', 111222333, 'Rua A, 100'),
('Empresa B', 444555666, 'Rua B, 200'),
('Empresa C', 777888999, 'Rua C, 300'),
('Empresa D', 123123123, 'Rua D, 400'),
('Empresa E', 456456456, 'Rua E, 500');

-- Inserir Assistências Externas
INSERT INTO AssistenciaExterna (Nome, NIF, Morada, Telefone) VALUES
('Assistência 1', 222333444, 'Rua X, 100', 911223344),
('Assistência 2', 555666777, 'Rua Y, 200', 922334455),
('Assistência 3', 888999000, 'Rua Z, 300', 933445566),
('Assistência 4', 111222333, 'Rua W, 400', 944556677),
('Assistência 5', 444555666, 'Rua V, 500', 955667788);

-- Inserir Máquinas
INSERT INTO Maquina (Nome, LinhaProd, AssistenciaExternaAssistenteID) VALUES
('Máquina A', 1, 1),
('Máquina B', 2, 2),
('Máquina C', 3, 3),
('Máquina D', 4, 4),
('Máquina E', 5, 5);

-- Inserir Encomendas de Clientes
INSERT INTO EncomendaCliente (DataEncomenda, Estado, ClienteClienteID) VALUES
('2024-03-10', 'Pendente', 1),
('2024-03-11', 'Confirmada', 2),
('2024-03-12', 'Pendente', 3),
('2024-03-13', 'Enviada', 4),
('2024-03-14', 'Cancelada', 5);

-- Inserir Itens nas Encomendas
INSERT INTO EncomendaItens (Quantidade, EncomendaClienteEncomendaClienteID) VALUES
(10, 1),
(5, 2),
(8, 3),
(6, 4),
(3, 5);


-- Inserir Ordens de Produção
INSERT INTO OrdemProducao (Estado, Quantidade, DataAbertura, MaquinaMaquinaID, EncomendaClienteEncomendaClienteID) VALUES
('Aberta', 20, '2024-03-15', 1, 1),
('Aberta', 15, '2024-03-16', 2, 2),
('Aberta', 10, '2024-03-17', 3, 3),
('Aberta', 25, '2024-03-18', 4, 4),
('Aberta', 12, '2024-03-19', 5, 5);


-- Inserir Produtos Acabados
INSERT INTO Produto (Nome, Quantidade, Descricao, CodInterno, Preco, OrdemProducaoOrdemProdID, EncomendaItensEncomendaItens) VALUES
('Produto X', 50, 'Produto feito com aço e plástico', 'P001', 120.00, 1, 1),
('Produto Y', 30, 'Produto com borracha e vidro', 'P002', 95.50, 2, 2),
('Produto Z', 40, 'Produto avançado com cobre', 'P003', 150.75, 3, 3),
('Produto W', 60, 'Produto genérico', 'P004', 80.20, 4, 4),
('Produto V', 20, 'Produto premium', 'P005', 200.00, 5, 5);

-- Inserir Matérias-Primas
INSERT INTO MateriaPrima (Nome, Quantidade, Descricao, Categoria, CodInterno, Preco) VALUES
('Aço', 500, 'Aço de alta resistência', 'Metais', 'MP001', 10.50),
('Plástico', 300, 'Plástico industrial', 'Polímeros', 'MP002', 5.75),
('Borracha', 200, 'Borracha sintética', 'Elastômeros', 'MP003', 7.25),
('Vidro', 100, 'Vidro temperado', 'Vidros', 'MP004', 12.00),
('Cobre', 250, 'Cobre refinado', 'Metais', 'MP005', 15.30);


--------

-- Relacionar Produtos e Matérias-Primas
INSERT INTO MateriaPrimaProduto (QuantidadeNec, MateriaPrimaMateriaPrimalD, ProdutoProdutoID) VALUES
(5, 1, 1),
(2, 2, 2),
(3, 3, 3),
(2, 4, 4),
(4, 5, 5);


-- Inserir Registos de Produção
INSERT INTO RegistoProducao (Estado, DataProducao, Observacoes, UtilizadorUtilizadorID, ProdutoProdutoID, OrdemProducaoOrdemProdID) VALUES
('Produzido', '2024-03-20', 'Desvio de 2 unidades', 1, 1, 1),
('Produzido', '2024-03-21', NULL, 2, 2, 2),
('Produzido', '2024-03-22', 'Sem problemas', 3, 3, 3),
('Produzido', '2024-03-23', '3 unidades rejeitadas', 4, 4, 4),
('Produzido', '2024-03-24', '2 unidades perdidas no teste de qualidade', 5, 5, 5);


