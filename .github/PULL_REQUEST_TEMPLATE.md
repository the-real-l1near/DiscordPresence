## Summary

Describe what this pull request changes.

## Related issue

Closes #

## App support checklist

- [ ] Windows process name verified
- [ ] Real window-title examples verified
- [ ] Project/workspace parser tested
- [ ] Discord display name reviewed
- [ ] Official icon/logo source provided or documented
- [ ] Discord asset key follows the project naming convention
- [ ] Production Discord asset uploaded or ready for maintainer upload

## Testing

- [ ] `dotnet build -c Release` succeeds without warnings or errors
- [ ] App process detection tested
- [ ] Project/workspace title parsing tested
- [ ] Switching between supported apps does not reset the session timer
- [ ] Idle behavior tested after closing supported apps
- [ ] Unsupported foreground apps preserve the last supported presence

## Notes

Add any implementation or testing notes here.
