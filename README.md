# Crud-S3

Api - Desenvolvida em .NET 8 - O código foi escrito para o usuário (existente no Amazon S3) poder enviar objetos, deletar, fazer o download e criar buckets.

Nos downloads, ele cria uma URL assinada e provisória que tem validade de 1 min. Após o download a URL perde a validade e o usuário não tem mais acesso aos objetos,

isso nos permite visualizar os objetos do Bucket, sem a necessidade de expor ele(s) a internet publica, não comprometendo a segurana do S3 ou a conta na AWS.

O código é pequeno mas considero fantastico.
