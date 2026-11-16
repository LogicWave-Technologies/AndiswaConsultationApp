# AndiswaConsultationApp

The mobile app for Andiswa Business Consultation (.NET MAUI + ASP.NET Core Web API + Azure SQL).
It uses the same database and admin as the website, but it has its **own design** and works like a phone app, not like a website.

## Test mode (no server needed)

Right now the app runs in **test mode**: it does **not** connect to any API, database or Google.
Everything works with pretend data saved on the device, so you can test every screen on its own.

- Switch: `Services/ApiConfig.cs` → `TestMode = true` (set it to `false` when the API, database and SSO are ready)
- The pretend server: `Services/FakeBackend.cs`
- Ready-made login: **demo@abc.test** / **Demo1234!** (or create your own account; it is saved on the device)
- **Continue with Google** waits 2 seconds and logs in a pretend Google user (no browser opens)
- Quote requests are saved on the device and show up under **My account**

## How the app is different from the website

| | Website | App |
|---|---|---|
| Colours | Dark navy, serious | Bright sky blue `#2D6CDF` + warm coral `#FF7A45` |
| Font | Website heading font | **Poppins** (rounded, made for small screens) |
| Navigation | Top menu with 7 links | Bottom bar: Home · Services · **(+ Quote)** · Work · More |
| Main action | "Get a Quote" link in the menu | Coral **floating Quote button** in the middle of the bottom bar, always one tap away |
| Home | Full-screen hero, long sections, footer, newsletter | Personal greeting ("Good morning, Ayanda"), compact quote card, quick actions, swipe rows, review carousel |
| Contact | One long form with a drop-down | **3-step quote wizard** with a progress bar, service **tiles**, details filled in for logged-in clients, "Request sent" screen |
| Portfolio | Gallery page with pop-up | **Masonry** picture grid (two columns, different heights), full-screen detail with a slide-up sheet |
| Footer | On every page | None: the bottom bar is always there instead |
| Account | Portal page | Profile header, counters and coloured **status chips** |
| More / Settings | — | Grouped list like the phone's Settings, dark-mode switch right on the page, quick answers open in place |

### Features only an app can offer (used here)

- **Call, WhatsApp, Email, Directions** buttons that open the phone's own apps (`Services/QuickContact.cs`)
- **Share** a design through the phone's share menu (Project detail page)
- **Haptic feedback**: a small vibration on important taps (`Services/Haptics.cs`)
- **Pull to refresh** and an **offline mode** (requests are saved and sent later)
- **Swipe** gestures: carousels and side-scrolling rows
- **Remembers you**: logged-in clients are greeted by name and their details are filled into the quote form
- **Dark mode** and font choice

## Login first

The app opens on the **Log in** screen. You must log in, create an account, or use **Continue with Google**
before you can reach Home. These two screens sit outside the tab bar in `AppShell.xaml`, so they have no bottom bar.
- If you logged in before, the app remembers you (`Services/ClientSession.cs`) and goes straight to Home.
- **Log out** (My account) takes you back to the Log in screen.

## Where things are

- `App.xaml` – the styles (buttons, cards, text) · `Services/ThemeService.cs` – the colours, light and dark
- `Controls/BottomNavBar.cs` – the bottom bar with the floating Quote button
- `Pages/` – one XAML + code file per screen
- `Models/Models.cs` – the content (services, projects, reviews …)

  ## Video Demonistration link
  https://youtu.be/2Ta_a6-rC14

  ## Azure link
  andiswa-business-api-buc0hwgybmhnabhs.southafricanorth-01.azurewebsites.net
  

The website-only parts (`SiteFooter`, `NewsletterBox`, `SectionHeader`) were removed from the app on purpose.
