# Scripts

Herramientas de cobertura de código para proyectos de test. Ejecutar desde la raíz del repositorio.

## Requisitos

- `dotnet` SDK 8+
- `reportgenerator` (tool global): `dotnet tool install -g dotnet-reportgenerator-globaltool`
- `coverlet.collector` referenciado en cada `.csproj` de tests

## coverage.sh

Ejecuta tests con cobertura, genera reporte HTML y muestra resumen por clase/método en consola.

```bash
# Uso
./scripts/coverage.sh <NombreProyectoTest>

# Ejemplos
./scripts/coverage.sh Fudie.Domain.UnitTests
./scripts/coverage.sh Fudie.Security.UnitTests
./scripts/coverage.sh Fudie.Api.UnitTests
```

**Salida:**
- Resumen de cobertura en consola (líneas, branches, métodos)
- Detalle por clase y método con porcentajes y complejidad
- Reporte HTML en `tests/<Proyecto>/TestResults/CoverageReport/index.html` (se abre automáticamente)

El filtro de ensamblado se deduce del nombre: `Fudie.Domain.UnitTests` filtra por `Fudie.Domain`.

## parse_cov.sh

Analiza un archivo `coverage.cobertura.xml` y muestra las líneas exactas sin cobertura (`UNCOVERED`) o con branches parciales (`PARTIAL`).

```bash
# Uso
./scripts/parse_cov.sh <ruta-cobertura.xml> [filtro]

# Ejemplos — todas las clases
./scripts/parse_cov.sh tests/Fudie.Security.UnitTests/TestResults/*/coverage.cobertura.xml

# Ejemplos — filtrar por nombre de clase
./scripts/parse_cov.sh tests/Fudie.Security.UnitTests/TestResults/*/coverage.cobertura.xml JwtValidator
```

**Salida:**
```
=== Fudie.Security.JwtValidator ===
Line rate: 91.7%  Branch rate: 82.3%

  Method: ExtractContext
    PARTIAL  line 37 branch=83.33% (5/6)
    PARTIAL  line 41 branch=83.33% (5/6)

  Method: ExtractStringArray
    UNCOVERED line 61
    UNCOVERED line 63
```

Solo muestra clases/métodos con problemas de cobertura. Si no hay salida, la cobertura es 100%.

## Flujo típico

```bash
# 1. Ejecutar tests + ver resumen general
./scripts/coverage.sh Fudie.Security.UnitTests

# 2. Investigar líneas exactas sin cubrir
./scripts/parse_cov.sh tests/Fudie.Security.UnitTests/TestResults/*/coverage.cobertura.xml
```
