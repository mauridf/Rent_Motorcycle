# Gestão de Locação de Motos para Entregadores (Rent_Motorcycle).
API implementada com .NET 7.0 C#.
Na parte relacionada a Upload de Imagens fiz uma Implementação que torna possível utiizar MinIO (S3) e Local Storage, quando ou se o MinIO não estiver operacional automaticamente ele assume o Local Storage.

## Instruções
- Fazer o Clone do Fonte ou Baixar.
- Abrir o projeto no Visual Studio 2022 (Foi o que utilizei para Implementar) ou se preferir no VSCode (lembrar de instalar as dependencias necessárias para C#).
- Verificar as dependencias e reinstalar as que forem necessárias.
- Alterar o arquivo appsettings de acordo com suas configurações.
- Criar apenas a base de dados vazia no Postgre (colocar o nome da base no appsettings, assim como as demais configurações de usuário conforme mencionei antes)
- Executar a Migration com o comando Update-Databe (no Console de Gerenciador de Pacotes) ou dotnet ef database update no CLI do .NET
- Executar a API e efetuar os testes nos Endpoints

## Tecnologias, Metodologias e Conhecimentos utilizados 🚀
- Testes unitários
- EntityFramework 
- Documentação
- Tratamento de erros
- Logs bem estruturados
- LocalStorage e MinIO
- RabittMQ (Messageria)
  
