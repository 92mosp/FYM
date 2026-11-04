# Contribución (`Contributing`)

1. Crea una rama desde `main` utilizando la convención `feature/<short-name>` o `fix/<short-name>`.
2. Mantén los cambios enfocados e incluye pruebas (*tests*) para los cambios de comportamiento.
3. Ejecuta `dotnet format FYM.sln`, `dotnet test FYM.sln`, `npm run lint`, `npm test` y `npm run build` antes de abrir un *pull request*.
4. Utiliza un *pull request* y requiere al menos una revisión aprobada.
5. Resuelve todos los hallazgos de CI, CodeQL, *dependency review*, *secret scan* y Trivy antes de realizar el *merge*.

Los *commits* deben ser pequeños, usar un asunto en imperativo y evitar credenciales o archivos generados por la compilación (*build output*).
