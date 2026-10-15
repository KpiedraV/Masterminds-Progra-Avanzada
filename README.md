# Masterminds-Progra-Avanzada

## Evaluador Mastermind

La biblioteca `Mastermind` implementa la evaluación de coincidencias exactas y
de colores correctos en posiciones distintas. Los nombres y códigos de color se
definen en un solo lugar: `Mastermind/colores.json`. También se pueden cargar
desde otro archivo JSON o proporcionar mediante `ConfiguracionColores`; no
forman parte del algoritmo. La configuración conserva `1` para Yellow y `N` para
Green como en el enunciado, y usa `I` para Pink para distinguirlo de Purple.
Los archivos JSON de colores y cheat codes son ejemplos configurables y se
copian al directorio de salida.

```csharp
var colores = ConfiguracionColores.DesdeArchivo("colores.json");
var repositorioCheats = new RepositorioCheatCodesJson("cheat-codes.json");
var sesion = new SesionPartida();
var evaluador = new EvaluadorMastermind(
    configuracionColores: colores,
    cheatCodeRepository: repositorioCheats,
    sesion: sesion);

var resultado = evaluador.EvaluarIntento("1CKP", "1KCP", "A1B2C3D4E5F6G7H8I9J0ABCDE");
```

Cada instancia de `SesionPartida` permite un solo uso válido de cheat code. Si
no se inyecta una configuración de colores, el evaluador conserva compatibilidad
con la API básica y no restringe los caracteres; las longitudes y el resto de
las entradas sí se validan. El evaluador escribe cada resultado al archivo
`evaluaciones.log` de su directorio de aplicación de forma predeterminada. El
logger, reloj, repositorio de cheats y generador aleatorio pueden inyectarse para
pruebas deterministas.