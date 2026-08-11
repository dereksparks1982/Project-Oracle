# Project Oracle World Time Intake — v0.0.16

The v0.0.16 cosmology and launcher build does not change the accepted world-time contract.

- four world seconds per real second;
- one Garden day per six real hours;
- four Garden days per real day;
- epoch Year 1, Month 1, Day 1, 01:01:01;
- world time never rewinds when system time moves backwards;
- closed-time catch-up applies forward once on restore;
- scheduled observations retain the event's scheduled world time rather than the later catch-up processing time.

Cosmology changes do not rewrite historical timestamps in existing saves.
