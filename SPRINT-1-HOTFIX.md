# Sprint 1 Hotfix 1

## Fixed

- Replaced the invalid positional-record compact constructor in `Card.cs` with an explicit record constructor.
- Preserved immutable card identity, definition ID, and owner ID properties.

## Reported compiler errors addressed

- CS1001
- CS1014
- CS1513
- CS8124
- CS1519

## Suggested commit

```text
AG-001-hotfix: fix Card record constructor syntax
```
