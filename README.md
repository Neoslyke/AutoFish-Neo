# AutoFish

## Description
Auto fishing plugin for TShock servers. Supports auto reel-in, multi-hook, Buffs, extra loot, consumption mode, etc. Commands can be shown/hidden dynamically via permissions and global switches.

- Legacy repo: https://github.com/ksqeib/AutoFish-old
- Remastered repo: https://github.com/UnrealMultiple/TShockPlugin/tree/master/src/AutoFish

## Player Commands (/af, /autofish)

| Command | Description | Permission | Prerequisite |
| --- | --- | --- | --- |
| /af | Show menu/help | autofish | Plugin enabled |
| /af status | Show personal status | autofish |  |
| /af fish | Toggle auto fishing | autofish.fish | Global auto fishing enabled |
| /af buff | Toggle fishing Buffs | autofish.buff | Global Buff enabled |
| /af multi | Toggle multi-hook | autofish.multihook | Global multi-hook enabled |
| /af hook <number> | Set personal hook cap | autofish.multihook | Global multi-hook enabled; value ≤ global cap |
| /af monster | Toggle avoid fishing monsters | autofish.filter.monster | Global anti-monster enabled |
| /af anim | Toggle skip catch animation | autofish.skipanimation | Global animation skip enabled |
| /af list | View consumption-mode items | autofish | Global consumption mode enabled |
| /af loot | View extra loot table | autofish | Extra loot list configured and non-empty |
| /af bait | Toggle precious bait protection | autofish.bait.protect | Global precious bait protection enabled |
| /af baitlist | View precious bait list | autofish.bait.protect | Same as above |

## Admin Commands (/afa, /autofishadmin)

All require `autofish.admin`.

| Command | Permission | Description |
| --- | --- | --- |
| /afa | `autofish.admin` | Show admin help menu |
| /afa buff | `autofish.admin` | Toggle global fishing Buff |
| /afa multi | `autofish.admin` | Toggle global multi-line mode |
| /afa duo \<number\> | `autofish.admin` | Set global multi-hook cap |
| /afa mod | `autofish.admin` | Toggle global consumption mode |
| /afa set \<amount\> | `autofish.admin` | Set consumption item quantity (when consumption mode is on) |
| /afa time \<minutes\> | `autofish.admin` | Set reward duration in minutes (when consumption mode is on) |
| /afa add \<item\> | `autofish.admin` | Add allowed bait (visible when consumption mode is on) |
| /afa del \<item\> | `autofish.admin` | Remove allowed bait (visible when consumption mode is on) |
| /afa addloot \<item\> | `autofish.admin` | Add extra loot |
| /afa delloot \<item\> | `autofish.admin` | Remove extra loot |
| /afa monster | `autofish.admin` | Toggle global avoid fishing monsters |
| /afa anim | `autofish.admin` | Toggle global skip catch animation |

## Config
See [resource/config/zh-cn.yml](resource/config/zh-cn.yml) or [resource/config/en-us.yml](resource/config/en-us.yml). When missing, the plugin writes a default template based on system language.

## Notes

- Simplest setup for `/af` for regular players: give group `autofish.common`. To disable a specific feature, additionally grant `autofish.no.<feature>`.
- With consumption mode on, players must have personal duration; the plugin returns early if bait is missing.
- Multi-hook/anti-monster/skip-animation all honor "global switch + personal switch + permission" simultaneously.

## Troubleshooting Guide

1. **Check plugin master switch**: ensure `pluginEnabled` is `true` in config.
2. **Check global feature switch**: ensure `globalAutoFishFeatureEnabled` is `true`.
3. **Check permissions**: player needs `autofish` and the feature permission (or `autofish.common`). `autofish.no.<feature>` forces denial.
4. **Check consumption mode**: if enabled, make sure exchange rules exist and the player has remaining duration.
5. **Check valuable bait protection**: it swaps bait to protect it. If issues occur, try disabling it with `/af bait`.
6. **Check bait**: without bait, auto fishing stops; refill and recast.
7. **Ensure SSC is enabled**: the plugin requires ServerSideCharacter to work properly.
8. **Enable debug**: admins run `/afa debug` to toggle debug mode.
9. **Reproduce and capture**: reproduce the fishing issue and keep screenshots of chat hints and console output for reporting.

## Mechanics (Behavior and Key Logic)

- Auto fishing: during bobber AI update, detect `bobber.ai[1] < 0` (caught), consume bait, call vanilla reel logic, then re-send projectile. If extra loot/monster filtering is enabled, filter/replace before drops spawn.
- Multi-hook: when spawning fishing line projectiles, count current bobbers; if under cap, duplicate a fishing line projectile for the player, enabling parallel fishing. Also gated by consumption mode and player multi-hook toggle.
- Skip catch animation: after reeling, send `ProjectileDestroy` to the client to skip the animation.
- Avoid fishing monsters: if result is a monster (catchId < 0) and feature is on, discard and retry.
- Protect precious bait: check current bait against precious list; if matched, swap with bait at the end of inventory and sync slots to prevent consumption.
- Consumption mode: when globally on, player must enable personally and have remaining duration to run auto fishing/multi-hook. Duration is exchanged via consuming specified items (commands and logic follow config fields).
- Buffs: when player has a fishing line and global/personal Buff is on, apply configured Buff list (ID + duration).
- Hint and first-fish: on first cast, prompt player that `/af fish` can enable auto fishing (once only).

## Feedback

- Issues: https://github.com/UnrealMultiple/TShockPlugin
- QQ: 816771079
- Community: trhub.cn / bbstr.net / tr.monika.love

## Changelog

- See CHANGELOG.md
