# Changelog

## 0.1.0

- Renamed public namespaces and most assembly definitions from `Voyage.ObservationToolkit` to `VoyageForge.ObservationToolkit`.
- Renamed the IL Post Processor assembly to `Unity.VoyageForge.ObservationToolkit.CodeGen` to preserve Unity ILPP compiler access.
- Added Command `CanExecute` dependency observation through `.Observes(...)` and `ObserveCanExecute(...)`.
- Added generic `UnityEvent` command binding through `BindCommand(target, unityEvent, command, parameter)`.
- Added stronger `RelayCommand<T>` parameter conversion using the shared binding conversion utility.
- Added EditMode coverage for command observation, parameter conversion, and Button command binding lifecycle.
- Added UPM package metadata for `com.voyageforge.observation-toolkit`.
