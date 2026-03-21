# AutoFish

## Description
Auto fishing plugin for TShock servers. Supports auto reel-in, multi-hook, Buffs, extra loot, consumption mode, etc. Commands can be shown/hidden dynamically via permissions and global switches.

- Legacy repo: https://github.com/ksqeib/AutoFish-old
- Remastered repo: https://github.com/UnrealMultiple/TShockPlugin/tree/master/src/AutoFish

## Player Commands
| Command | Permission | Description |
| --- | --- | --- |
| /af | `autofish` | Show menu/help |
| /af status | `autofish` | Show personal status |
| /af fish | `autofish.fish` | Toggle auto fishing |
| /af buff | `autofish.buff` | Toggle fishing Buffs |
| /af multi | `autofish.multihook` | Toggle multi-hook |
| /af hook \<number\> | `autofish.multihook` | Set personal hook cap |
| /af monster | `autofish.filter.monster` | Toggle avoid fishing monsters |
| /af anim | `autofish.skipanimation` | Toggle skip catch animation |
| /af list | `autofish` | View consumption-mode items |
| /af loot | `autofish` | View extra loot table |
| /af bait | `autofish.bait.protect` | Toggle precious bait protection |
| /af baitlist | `autofish.bait.protect` | View precious bait list |

## Admin Commands
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