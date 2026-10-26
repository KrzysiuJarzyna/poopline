# Sprawdzenie Poopline 0.4.0

26 października 2026. Unity **6000.5.1f1**, macOS, lokalny Unity Editor.

## Rozgrywka

Pełny zestaw **16/16 testów PlayMode**, zero błędów, pominięć i testów nieokreślonych. Czas zestawu: **363.86 s**. Raport: `TestResults/PlayMode-0.4.xml`, zakończony 26.10.2026 o 14:06:49 UTC.

- Pełne przejście pierwszego etapu, automatyczne uruchomienie drugiego i ukończenie drugiego na rzeczywistym kontrolerze, klawiaturze oraz kolizjach. Bez teleportowania postaci do kolejnych fragmentów i bez awaryjnych powrotów.
- Zmierzony czas gry znaną trasą: **156.1 s / 2:36** dla pierwszego etapu, **204.4 s / 3:24** dla drugiego. W teście pełnej trasy zegar Unity działał z mnożnikiem 2, bez zmiany prędkości postaci, drabin lub nurtu. Podane wyniki to czas gry, nie czas oczekiwania automatu. Nie obejmują szukania drogi i oglądania bocznych komór; pierwsze przejście przez gracza może być dłuższe. Raport: `TestResults/stage-times.txt`.
- Start pierwszego etapu bez przeciwników, nieaktywny drugi etap, brak napisów TextMesh w obu światach.
- Chodzenie, zatrzymanie, cofanie; nurt znoszący postać, ruch pod prąd i wyjście na oba brzegi w trzech miejscach.
- Wejście do bocznego kanału, brodzenie w nim, przejście do następnego odcinka oraz zapis punktu kontrolnego.
- Wszystkie sześć górnych powierzchni: próby wyjścia za krawędź w ośmiu kierunkach, bez spadnięcia do wody. Kolizje widocznych poręczy uzupełnia ochrona otworów przy drabinach.
- Wszystkie sześć drabin: wejście i zejście zgodnie z ich ustawieniem, zatrzymanie, cofanie, pauza i oczekiwanie na puszczenie kierunku po dotarciu na podest.
- Automatyczny chwyt bocznej drabiny przy podejściu z górnej platformy. Odejście po skosie po zejściu nie przyciąga ponownie na drabinę — osobny test regresji dla wszystkich sześciu drabin.
- Kontakt ze szczurem przywraca właściwy punkt kontrolny bieżącego poziomu. Pierwszy etap nie ma szczurów; drugi ma pięć patroli.
- Pauza zatrzymuje ruch, patrole, krople i zegar. Restart drugiego etapu pozostaje w drugim. Restart po ukończeniu całości wraca do pierwszego.
- Przejście między etapami zatrzymuje sterowanie; można je zatrzymać pauzą. Dopiero wyjście z drugiego poziomu kończy całość.
- Kamera podąża także w boczne odnogi. Utrata fokusu wstrzymuje grę i nie wznawia jej automatycznie.
- Telefon podąża za obrotem postaci. Brak słońca i światła otoczenia; dalekie lampy pozostają wygaszone i odsłaniają się po podejściu.

## Widoczność i obraz

Po sprawdzeniu pełnej trasy poprawiono wyłącznie zasłanianie postaci przez nadproża i sklepienia bocznych kanałów. Odsłaniane elementy zachowują kolizje oraz rzucane cienie. Dla tej poprawki osobno przeszły **4/4 testy** kamery, telefonu, przejścia przez boczny kanał i przywracania widoczności nadproża po wyjściu. Czas: 15.94 s; zakończenie: 2026-10-26 14:15:10Z. Raport: `TestResults/Visibility-0.4.xml`.

Ręcznie w Unity sprawdzono uruchomienie przez Play, chodzenie i brodzenie oraz wygląd telefonu, cieni, wody i ceglanego kanału. Obrazy z rzeczywistej rozgrywki testowej sprawdzono także dla górnego pomostu i bocznego tunelu: `TestResults/stage-1.png`, `stage-2.png`, `side-channel.png`. Pełną drogę przez oba etapy potwierdza automatyczny test ruchu, nie ręczna sesja.

Scenę waliduje **Tools → Poopline → Validate playable canal**: dwa etapy, jedna postać, zero szczurów w pierwszym, pięć w drugim, sześć drabin, brak napisów w świecie i brak brakujących skryptów. Zapisana scena zawiera edytowalne obiekty obu etapów; tylko pierwszy jest aktywny przy starcie.

## Zakres

- Dwa etapy w jednej scenie startowej. Nie ma trzeciego poziomu ani mapy miasta.
- Woda i krople są animowaną geometrią z lokalnymi prądami, bez symulacji hydraulicznej lub pływania. Wloty i odpływy mają dalsze odcinki za kratą oraz spadki do ciemnych komór; boczne kanały są dostępne dla postaci.
- Zachowano prototypowe materiały, sylwetkę oraz animacje. Dodano murowane łuki, odpływy z rur, kraty, żeliwne włazy, koła zasuw i pompy. Brak nagrań dźwiękowych.
- Punkty kontrolne obowiązują w bieżącej rozgrywce; brak zapisu kampanii między uruchomieniami.
- Silnik i pakiety bez aktualizacji. Nie wykonano nowego buildu macOS ani testów na iOS/Androidzie. Stare aplikacje w `Builds` nie przedstawiają tej wersji.
- Raporty i obrazy w `TestResults` są lokalne i ignorowane przez Git. Kopia stanu sprzed rozbudowy jest w `Library/PooplineBackups/before-two-levels-20261026.tar.gz`.
- Nie utworzono commitów, nie zmieniano historii Git, nie wykonano push ani stagingu.
