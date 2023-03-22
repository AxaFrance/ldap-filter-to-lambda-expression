# AxaFrance.LdapFiltersToLambdaExpression

Translate ldap filters (string) to lambda expression. it is compatible with EFCore in order to generate an SQL query
from an ldap filter

## Usage

### Configuration for EFCore

Cette configuration est requise seulement si vous souhaitez requeter dans un model de donnée SQL via EntityFramework.

EFCoreLike permet dans le cas d'EFCore d'utiliser la methode Like d'EntityFramework et non pas celle definit par defaut
dans cette librairie

```csharp
    public class EFCoreLike : IFunction
    {
        public MethodInfo MethodInfo => this.DeclaringType.GetMethod(nameof(DbFunctionsExtensions.Like),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

        public Type DeclaringType => typeof(DbFunctionsExtensions);

        public string ExpressionPattern =>
            $"{nameof(DbFunctionsExtensions)}.{this.MethodInfo.Name}({nameof(EF)}.{nameof(EF.Functions)}, {{0}},{{1}})";
    }
```

EFCoreSoundex permet dans le cas d'EFCore d'utiliser une methode gerant une approximationcompatible avec SQL et non pas
celle definit par defaut dans cette librairie

```csharp
    public class EFCoreSoundex : IFunction
    {
        public MethodInfo MethodInfo => this.DeclaringType.GetMethod(nameof(SampleDbContext.Soundex),
            new[] { typeof(string) })!;

        public Type DeclaringType => typeof(SampleDbContext);

        public string ExpressionPattern => $"{this.DeclaringType.FullName}.{this.MethodInfo.Name}({{0}})";
    }
```

Il est important d'ajouter les namespaces neccessaire a la generation de lanbda, ici le namespace EntityFrameworkCore

```csharp
    services.LdapFiltersToLambdaConfigure(option =>
    {
        option.LikeFunction(new EFCoreLike())
            .ApproximateFunction(new EFCoreSoundex())
            .AddNamespaces($"{nameof(Microsoft)}.{nameof(Microsoft.EntityFrameworkCore)}");
    })
```

### Queries

#### Where Query

```csharp
     var whereQuery = queryable.ByLdapFilterAsync("(job=dirige*)");
```

#### Specify Query

```csharp
     var singleElement = queryable.ByLdapFilterAsync("(job=dirige*)", Queryable.Single);
     var singleOrDefaultElement = queryable.ByLdapFilterAsync("(job=dirige*)", Queryable.SingleOrDefault);
     var firstElement = queryable.ByLdapFilterAsync("(job=dirige*)", Queryable.First);
     var firstOrDefaultElement = queryable.ByLdapFilterAsync("(job=dirige*)", Queryable.FirstOrDefault);
     ...
```
