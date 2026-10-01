# TacticalFPS Foundation

Unity için CS tarzı rekabetçi FPS çekirdeği. Otoriter sunucu, istemci girdilerinin zaman damgalı sırayla işlenmesi ve görsel simülasyonun sabit simülasyon hızından ayrılması üzerine tasarlanmıştır.

## Mimari Tanım

- **Core:** Ağdan bağımsız zaman, input komutu ve takımlı aktör sözleşmeleri.
- **Movement:** İvme/sürtünme temelli, sabit adım (fixed-step) hareket ve crouch-jump/bhop pencereleri.
- **Combat:** Veri odaklı silah tanımları, recoil/spread, rollback geçmişi, hitscan ve malzeme nüfuzu.
- **Match:** MR12 raund yaşam döngüsü ve economy state machine.
- **Utility:** Patlayıcı arayüzleri ve deterministik hasar/etki hesapları.

## Sistem Akışı

1. İstemci her giriş değişikliğini `InputCommand` ile monotonic zaman damgası taşıyarak sunucuya yollar.
2. Sunucu `ServerInputBuffer` içinden, simülasyon anına kadar olan komutları sıralı tüketir.
3. `CompetitiveMotor` aynı sabit simülasyon adımında hız, sürtünme, sıçrama ve tag slow uygular.
4. Atışta sunucu, istemci atış anını izin verilen pencereye kısaltır, `PoseHistory` ile hedefleri o ana rollback eder ve hitscan/penetration uygular.
5. Sonuçlar sunucu tarafından doğrulanıp istemciye yalnızca olay/snapshot olarak yayınlanır.

> `IHitscanWorld`, `IMatchEventSink`, `ICharacterSpawnService` gibi sınırlar Netcode for GameObjects, Mirror veya özel transport adaptörü ile doldurulmalıdır. Oyun kuralları transport katmanına bağlı değildir.
