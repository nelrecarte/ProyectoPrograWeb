# ProyectoPrograWeb

Backend del proyecto en .NET 10 con Firebase (Auth + Firestore).

## Integrantes

| Nombre | Rama |
|---|---|
| Nelson Recarte | `feat/nelson-setup` |
| Dorali Hernandez| `feat/dorali-setup` |
| Keren Padilla | `feat/keren-setup` |
| Melvin Rosalez| `feat/melvin-setup` |
| Nicoll Perez| `feat/nicoll-setup` |

## Cómo correrlo

1. Clonar el repo
2. Pedirle a Nelson el archivo `firebase-key.json` y ponerlo dentro de `ProyectoPrograWeb/` (no se sube al repo porque tiene las credenciales)
3. Correr:

```bash
cd ProyectoPrograWeb
dotnet restore
dotnet run
```

Corre en `http://localhost:5099`

## Probar la conexión con Firebase

```
http://localhost:5099/api/test/firebase-check/{nombre}
```

Ejemplo: `http://localhost:5099/api/test/firebase-check/keren`

Devuelve un JSON con status ok y crea un documento en la colección `test` de Firestore. Cada quien usa su nombre para que quede registro de que su entorno conecta bien.

## Reglas de la rama main

`main` está protegida: no se puede hacer push directo y todo PR necesita 2 aprobaciones antes de mergear.

## Flujo de trabajo

```bash
git checkout main
git pull origin main
git checkout -b feat/tu-nombre
git push -u origin feat/tu-nombre
```

Al terminar se abre PR hacia `main` y se esperan las 2 aprobaciones.