# Contribución (`Contributing`)

1. Crea una rama desde `main` utilizando la convención `feature/<short-name>` o `fix/<short-name>`.
2. Mantén los cambios enfocados e incluye pruebas (*tests*) para los cambios de comportamiento.
3. Antes de abrir un *pull request*, ejecuta los checks del [README](README.md#tests-y-checks): `dotnet format`, `dotnet build`, `dotnet test` con cobertura, y en `Front` `npm run format:check`, `npm run lint`, `npm test` y `npm run build`.
4. Utiliza un *pull request* y requiere al menos una revisión aprobada.

Los *commits* deben ser pequeños, usar un asunto en imperativo y evitar credenciales o archivos generados por la compilación (*build output*).
