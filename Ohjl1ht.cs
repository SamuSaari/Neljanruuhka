using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// <summary>
/// @author Samu Saari
/// Version 22.04.2020
/// Autopeli, jossa tavoitteena ohittaa
/// mahdollisimman monta autoa törmäämättä
/// </summary>
public class Ohjl1ht : PhysicsGame
{
    private const double PELAAJAN_NOPEUS = 300;

    private Vector impulssi = new Vector(0, -7000);

    private Vector hirviimpulssi = new Vector(-3000, -1000);

    private readonly EasyHighScore topLista = new EasyHighScore();

    private DoubleMeter elamaLaskuri;

    private PhysicsObject auto;

    private PhysicsObject alareuna;

    private IntMeter pisteLaskuri;

    private List<int> hirvienmaara = new List<int>();

    private readonly SoundEffect tormausaani = LoadSoundEffect("tormaus");
    private readonly SoundEffect kaynnistys = LoadSoundEffect("kaynnistys");

    private readonly int scrollausnopeus = -40; //nopeus

    private List<GameObject> taustakuvat;
    private Timer taustaAjastin = new Timer();
    private GameObject ekaTaustakuva;

    private List<Label> valikonKohdat;

    /// <summary>
    /// Ladataan alkuvalikko peli käynnistettäessä
    /// </summary>
    public override void Begin()
    {
        Alkuvalikko();
    }


    /// <summary>
    /// luodaan alkuvalikkoon 4 kohtaa
    /// aloita peli
    /// mahdoton pelimuoto
    /// pistetaulukon tarkastelu
    /// poistuminen
    /// </summary>
    private void Alkuvalikko()
    {
        MediaPlayer.Play("alkuvalikko");
        MediaPlayer.IsRepeating = true;

        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Start Game");
        kohta1.Position = new Vector(0, 40);
        valikonKohdat.Add(kohta1);
        kohta1.TextColor = Color.Cyan;
        kohta1.Color = Color.Black;

        Label kohta2 = new Label("Exit");
        kohta2.Position = new Vector(0, -80);

        valikonKohdat.Add(kohta2);
        kohta2.TextColor = Color.Cyan;
        kohta2.Color = Color.Black;

        Label kohta3 = new Label("Impossible mode");
        kohta3.Position = new Vector(0, 0);
        valikonKohdat.Add(kohta3);
        kohta3.TextColor = Color.BloodRed;
        kohta3.Color = Color.Black;

        Label kohta4 = new Label("Parhaat Pisteet");
        kohta4.Position = new Vector(0, -40);
        valikonKohdat.Add(kohta4);
        kohta4.TextColor = Color.Cyan;
        kohta4.Color = Color.Black;

        Level.Background.Image = LoadImage("Alotuskuva");
        Level.Background.FitToLevel();

        Camera.ZoomFactor = 1.1;

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, AloitaPeli, null);
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, Exit, null);
        Mouse.ListenOn(kohta3, MouseButton.Left, ButtonState.Pressed, HcMode, null);
        Mouse.ListenOn(kohta4, MouseButton.Left, ButtonState.Pressed, ParhaaPisteet, null);

        Mouse.IsCursorVisible = true;
        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, null);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "");
    }


    /// <summary>
    /// määritellään mitä teksti tekee, kun sen päälle viedään hiiri
    /// </summary>
    private void ValikossaLiikkuminen(AnalogState hiirenTila)
    {
        foreach (Label kohta in valikonKohdat)
        {
            if (Mouse.IsCursorOn(kohta))
            {
                kohta.TextColor = Color.Red;

            }
            else
            {
                kohta.TextColor = Color.Cyan;
            }
        }
    }


    /// <summary>
    /// Aloitetaan peli
    /// Ladataan kaikki elementit peliin
    /// ja luodaan ajastimet tietokoneautojen ja kuoppien luomiseen
    /// laitetaan myös taustamusiikki päälle
    /// </summary>
    private void AloitaPeli()
    {
        kaynnistys.Play();
        ClearAll();
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Alkuvalikko, "Palaa valikkoon");
        Level.BackgroundColor = Color.Gray;
        LuoKentta();

        LuoTaustakuvat();

        LuoPistelaskuri();

        AsetaOhjaimet();

        LuoElamaLaskuri();

        Timer ajastin = new Timer();
        ajastin.Interval = 0.65;

        ajastin.Timeout += LuoTietokoneAuto;
        ajastin.Start();

        Timer kuopanajastin = new Timer();
        kuopanajastin.Interval = 1.5;
        kuopanajastin.Timeout += Kuoppa;
        kuopanajastin.Start();

        MediaPlayer.Play("ingame_01");
        MediaPlayer.IsRepeating = true;
    }


    /// <summary>
    /// näyttää pelin Top-listan
    /// </summary>
    private void ParhaaPisteet()
    {
        topLista.Show();
    }


    /// <summary>
    /// mahdoton pelimuoto, täysi kaos
    /// autojen luomisnopeus minimoitu
    /// </summary>
    private void HcMode()
    {
        ClearAll();
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Alkuvalikko, "Palaa valikkoon");
        Level.BackgroundColor = Color.Gray;
        LuoKentta();

        LuoTaustakuvat();

        LuoPistelaskuri();

        AsetaOhjaimet();

        LuoElamaLaskuri();


        Timer ajastin = new Timer();
        ajastin.Interval = 0.3;

        ajastin.Timeout += LuoTietokoneAuto;
        ajastin.Start();


        Timer kuopanajastin = new Timer();
        kuopanajastin.Interval = 1.5;
        kuopanajastin.Timeout += Kuoppa;
        kuopanajastin.Start();

        MediaPlayer.Play("ingame_01");
        MediaPlayer.IsRepeating = true;
    }


    /// <summary>
    /// luodaan taustakuvista lista
    /// ja toistetaan yhtä kuvaa, joka muodostaa illuusion
    /// loputtomasta tiestä
    /// </summary>
    private void LuoTaustakuvat()
    {

        taustaAjastin = new Timer();
        taustaAjastin.Interval = 0.03;   // nopeus
        taustaAjastin.Timeout += LiikutaTaustaa;
        taustaAjastin.Start();

        taustakuvat = new List<GameObject>();
        LisaaTaustakuva("kuva1", 1024, 768);
        LisaaTaustakuva("kuva2", 1024, 768);
        LisaaTaustakuva("kuva3", 1024, 768);
        LisaaTaustakuva("kuva4", 1024, 768);
    }


    /// <summary>
    /// monistetaan taustakuva
    /// </summary>
    /// <param name="nimi">kuvan nimi</param>
    /// <param name="leveys">kuvan leveys</param>
    /// <param name="korkeus">kuvan korkeus</param>
    private void LisaaTaustakuva(string nimi, double leveys, double korkeus)
    {
        GameObject olio = new GameObject(leveys, korkeus);
        olio.Image = LoadImage("kaistaviivat");
        olio.X = 0;
        Add(olio);

        if (taustakuvat.Count > 0)
        {
            olio.Top = taustakuvat[taustakuvat.Count - 1].Bottom;
            if (scrollausnopeus >= 0) ekaTaustakuva = olio;
        }


        else
        {
            olio.Top = Level.Top;
            if (scrollausnopeus < 0) ekaTaustakuva = olio;
        }


        taustakuvat.Add(olio);

    }


    /// <summary>
    /// siirrellään taustoja alhaalta ylös muodostaen
    /// loputtoman kentän efektin
    /// </summary>
    private void LiikutaTaustaa()
    {
        foreach (GameObject taustakuva in taustakuvat)
        {
            taustakuva.Y += scrollausnopeus;

            if (scrollausnopeus < 0 && taustakuva.Top < Level.Bottom)
            {
                taustakuva.Bottom = ekaTaustakuva.Top;
                ekaTaustakuva = taustakuva;
            }
            else if (scrollausnopeus > 0 && taustakuva.Bottom > Level.Top)
            {
                taustakuva.Top = ekaTaustakuva.Bottom;
                ekaTaustakuva = taustakuva;
            }
        }

    }


    /// <summary>
    /// luokaan kentälle reunat
    /// ja pelaajan auto
    /// </summary>
    private void LuoKentta()
    {

        Level.Background.Color = Color.Gray;
        Camera.ZoomToLevel();

        Level.Background.FitToLevel();

        double reunankitka = 0.0;

        PhysicsObject vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 1.0;
        vasenReuna.KineticFriction = reunankitka;
        vasenReuna.IsVisible = false;

        PhysicsObject oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.KineticFriction = reunankitka;
        oikeaReuna.IsVisible = false;

        PhysicsObject ylaReuna = Level.CreateTopBorder();
        ylaReuna.Restitution = 1.0;
        ylaReuna.KineticFriction = reunankitka;
        ylaReuna.IsVisible = false;

        alareuna = Level.CreateBottomBorder();
        alareuna.Restitution = 0.0;
        alareuna.KineticFriction = reunankitka;
        alareuna.IsVisible = false;

        auto = PelaajanAuto(Level.Bottom + 300, -300);
    }


    /// <summary>
    /// Luodaan pelaajan liikuteltava auto
    /// lisätään autolle myös törmäystapahtumat
    /// </summary>
    /// <param name="x">auton x-koordinaatti</param>
    /// <param name="y">auton y-koordinaatti</param>
    /// <returns></returns>
    private PhysicsObject PelaajanAuto(double x, double y)
    {
        PhysicsObject auto = new PhysicsObject(92, 153);
        auto.Shape = Shape.Rectangle;
        auto.Color = Color.Gold;
        auto.X = x;
        auto.Y = y;
        auto.IgnoresGravity = true;
        auto.IgnoresCollisionResponse = true;
        //Auto.Position = sijainti;
        auto.Restitution = 1.0;
        auto.KineticFriction = 0.0;
        auto.Image = LoadImage("pelaajanauto");

        AddCollisionHandler(auto, "muutAutot", PelaajaTormasi);
        AddCollisionHandler(auto, "kuoppa", AjaKuoppaan);

        Add(auto, 1);
        return auto;
    }


    /// <summary>
    /// luodaan vihollisautot syntymään
    /// satunnaisiin rajattuihin x-koordinaatteihin
    /// Auton osuessa alareunaan tapahtuu ohitus
    /// eli pistelaskuriin lisätään +1
    /// ja auto tuhotaan kentältä
    /// </summary>
    private void LuoTietokoneAuto()
    {
        int x = RandomGen.SelectOne(-295, -75, 75, 295, 70, -70, 100, -100);
        Vector paikka = new Vector(x, 400);

        PhysicsObject npcAuto = new PhysicsObject(92, 153);
        npcAuto.Shape = Shape.Rectangle;
        npcAuto.Position = paikka;
        npcAuto.Image = LoadImage("npc");
        npcAuto.Mass = 20.0;
        npcAuto.Color = Color.Blue;
        npcAuto.Tag = "muutAutot";
        npcAuto.IgnoresGravity = false;
        npcAuto.Hit(impulssi);

        AddCollisionHandler(npcAuto, alareuna, CollisionHandler.DestroyObject);
        AddCollisionHandler(npcAuto, alareuna, CollisionHandler.AddMeterValue(pisteLaskuri, 1));
        AddCollisionHandler(npcAuto, "elain", Hirvikolari);

        Add(npcAuto);
    }


    /// <summary>
    /// Luodaan kentälle kuoppa, johon osuessa pelaajan
    /// auton elämäpisteet vähenee
    /// </summary>
    private void Kuoppa()
    {

        int x = RandomGen.SelectOne(-390, 385);
        Vector kuopanpaikka = new Vector(x, 400);

        PhysicsObject kuoppa = new PhysicsObject(30, 30);
        kuoppa.Image = LoadImage("kuoppa");
        kuoppa.Shape = Shape.Circle;
        kuoppa.Mass = 20.0;
        kuoppa.Color = Color.DarkGray;
        kuoppa.Position = kuopanpaikka;
        kuoppa.Tag = "kuoppa";
        kuoppa.IgnoresGravity = false;
        kuoppa.Hit(impulssi);
        AddCollisionHandler(kuoppa, alareuna, CollisionHandler.DestroyObject);
        Add(kuoppa, 0);

    }


    /// <summary>
    /// Tapahtuma pelaajan törmätessä toiseen autoon
    /// </summary>
    /// <param name="pelaajanauto">pelaajan ohjaama auto</param>
    /// <param name="NpcAuto">Tietokoneauto</param>
    private void PelaajaTormasi(PhysicsObject pelaajanauto, PhysicsObject NpcAuto)
    {
        ClearAll();
        tormausaani.Play();
        pelaajanauto.Destroy();
        MessageDisplay.Add("Pelaaja törmäsi!, pisteesi " + pisteLaskuri);
        MessageDisplay.Add("Tiellä olleiden hirvien määrä: " + hirvienmaara.Count);
        NpcAuto.Destroy();

        Explosion rajahdys = new Explosion(auto.Width * 3);
        rajahdys.Position = auto.Position;
        rajahdys.UseShockWave = false;
        Add(rajahdys);

        topLista.EnterAndShow(pisteLaskuri.Value);
        topLista.HighScoreWindow.Closed += Alkuvalikkoon;
    }


    /// <summary>
    /// Palauttaa näkymän alkuvalikkoon
    /// ja tyhjentää kentältä muut elementit
    /// </summary>
    /// <param name="sender"></param>
    private void Alkuvalikkoon(Window sender)
    {
        ClearAll();
        Alkuvalikko();
    }


    /// <summary>
    /// luo pelaajalle elämäpisteet
    /// arvolla 3
    /// </summary>
    private void LuoElamaLaskuri()
    {
        elamaLaskuri = new DoubleMeter(10);
        elamaLaskuri.MaxValue = 3;
        elamaLaskuri.LowerLimit += ElamaLoppui;

        ProgressBar elamaPalkki = new ProgressBar(150, 20);
        elamaPalkki.X = Screen.Left + 850;
        elamaPalkki.Y = Screen.Top - 20;
        elamaPalkki.BindTo(elamaLaskuri);
        Add(elamaPalkki);
    }


    /// <summary>
    /// Pelaajan osuessa kuoppaan
    /// Elämäpalkista väjentyy 1 piste
    /// </summary>
    /// <param name="pelaajanauto">Pelaajan ohjaama auto</param>
    /// <param name="Kuoppa">Kuoppa</param>
    private void AjaKuoppaan(PhysicsObject pelaajanauto, PhysicsObject Kuoppa)
    {
        elamaLaskuri.Value -= 1;
        Kuoppa.Destroy();
        MessageDisplay.Add("Varo Kuoppaa!!");
    }


    /// <summary>
    /// Elämän loppuessa näytetään viesti ja
    /// tuhotaan auto
    /// näytetään myös pisteet
    /// </summary>
    private void ElamaLoppui()
    {
        ClearAll();

        MessageDisplay.Add("Auto Hajosi!");
        auto.Destroy();
        topLista.EnterAndShow(pisteLaskuri.Value);
        topLista.HighScoreWindow.Closed += Alkuvalikkoon;
    }


    /// <summary>
    /// luodaan pistelaskuri
    /// </summary>
    private void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        pisteLaskuri.MaxValue = 10000;
        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + 700;
        pisteNaytto.Y = Screen.Top - 20;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = "Pisteet";

        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }


    /// <summary>
    /// Aliohjelma luo kentälle 3 hirveä kutsuttaessa
    /// satunnaisiin paikkoihin
    /// </summary>
    private void Hirvi()
    {
        for (int i = 0; i < 3; i++)
        {
            PhysicsObject hirvi = new PhysicsObject(100, 100);
            hirvi.Image = LoadImage("hirvikuva");
            hirvi.Shape = Shape.Rectangle;
            hirvi.Mass = 20.0;
            hirvi.Hit(hirviimpulssi);
            hirvi.Tag = "elain";
            hirvi.Color = Color.Brown;
            hirvi.Position = RandomGen.NextVector(Level.Top, Level.Right, Level.Left, Level.Bottom);
            AddCollisionHandler(hirvi, "muutAutot", Hirvikolari);
            AddCollisionHandler(hirvi, "kuoppa", Hirvikolari);
            hirvienmaara.Add(1);
            Add(hirvi, 3);
        }
    }


    /// <summary>
    /// hirvet räjäyttävät objektin osuessaan niihin
    /// </summary>
    /// <param name="hirvi">hirvi</param>
    /// <param name="npcAuto">tietokoneluodut autot</param>
    private void Hirvikolari(PhysicsObject hirvi, PhysicsObject npcAuto)
    {
        npcAuto.Destroy();
        hirvi.Destroy();
        Explosion rajahdys = new Explosion(hirvi.Width * 1.5);
        rajahdys.Position = hirvi.Position;
        rajahdys.UseShockWave = false;
        Add(rajahdys);
    }


    /// <summary>
    /// Asetetaan pelaajan autolle ohjaimet
    /// </summary>
    private void AsetaOhjaimet()
    {
        Vector vasen = new Vector(-PELAAJAN_NOPEUS, 0);
        Vector oikea = new Vector(PELAAJAN_NOPEUS, 0);
        Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeus, "Liikuta autoa vasemmalle", auto, vasen);
        Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, auto, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeus, "Liikuta autoa oikealle", auto, oikea);
        Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, auto, Vector.Zero);
        Keyboard.Listen(Key.Space, ButtonState.Pressed, Hirvi, null);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// asetetaan pelaajan autolle liikkumisnopeuden kiihtyvyys ja suunta
    /// </summary>
    /// <param name="PelaajanAuto">Pelaajan Ohjaama auto</param>
    /// <param name="nopeus">Aiemmin määritelty nopeusvektori</param>
    private void AsetaNopeus(PhysicsObject PelaajanAuto, Vector nopeus)
    {
        if ((nopeus.X < 0) && (PelaajanAuto.Left < Level.Left))
        {
            PelaajanAuto.Velocity = Vector.Zero;
            return;
        }
        if ((nopeus.X > 0) && (PelaajanAuto.Right > Level.Right))
        {
            PelaajanAuto.Velocity = Vector.Zero;
            return;
        }

        PelaajanAuto.Velocity = nopeus;
    }
}