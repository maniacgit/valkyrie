﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroCanvas : MonoBehaviour {

    public float offset;
    public Dictionary<int, UnityEngine.UI.Image> icons;
    public static float heroSize = 4;
    public static float offsetStart = 3.75f;

    public void SetupUI() {
        icons = new Dictionary<int, UnityEngine.UI.Image>();
        offset = offsetStart;
        Game game = Game.Get();
        foreach (Round.Hero h in game.round.heroes)
            AddHero(h, game);
    }

    void AddHero(Round.Hero h, Game game)
    {
        Sprite heroSprite;

        string heroName = h.id.ToString();
        Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;

        if (h.heroData != null)
        {
            newTex = ContentData.FileToTexture(h.heroData.image);
            heroName = h.heroData.name;
        }

        GameObject heroImg = new GameObject("heroImg" + heroName);
        heroImg.tag = "herodisplay";

        heroImg.transform.parent = game.uICanvas.transform;

        RectTransform trans = heroImg.AddComponent<RectTransform>();

        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (0.25f + offset) * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());
        offset += heroSize + 0.5f;
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.25f * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());
        heroImg.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image image = heroImg.AddComponent<UnityEngine.UI.Image>();
        icons.Add(h.id, image);
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(heroSize * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());

        UnityEngine.UI.Button button = heroImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { HeroDiag(h.id); });
    }

    public void UpdateStatus()
    {
        Game game = Game.Get();
        foreach(Round.Hero h in game.round.heroes)
        {
            UnityEngine.UI.Image image = icons[h.id];
            image.color = Color.white;
            if (h.defeated)
            {
                image.color = Color.red;
            }
            if (h.activated)
            {
                image.color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
            }
            if (h.defeated && h.activated)
            {
                image.color = new Color((float) 0.2, 0, 0, 1);
            }
            if (h.heroData == null)
            {
                image.color = Color.clear;
            }
            if (h.selected)
            {
                image.color = Color.green;
            }
        }
    }

    public void UpdateImages()
    {
        Game game = Game.Get();
        foreach (Round.Hero h in game.round.heroes)
        {
            UnityEngine.UI.Image image = icons[h.id];

            Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;

            if (h.heroData != null)
            {
                newTex = ContentData.FileToTexture(h.heroData.image);
            }

            Sprite heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = heroSprite;
            image.color = Color.white;
        }
    }

    void HeroDiag(int id)
    {
        Game game = Game.Get();
        Round.Hero target = null;

        foreach (Round.Hero h in game.round.heroes)
        {
            if (h.id == id)
            {
                target = h;
            }
        }

        // If there are any other dialogs
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            if (game.round.eventList.Count > 0 && game.round.eventList.Peek().maxHeroes != 0)
            {
                target.selected = !target.selected;
                UpdateStatus();
            }
            return;
        }

        if (game.round.heroesSelected && target.heroData != null)
        {
            new HeroDialog(target);
        }
        if (!game.round.heroesSelected)
        {
            icons[id].color = new Color((float)0.3, (float)0.3, (float)0.3);
            new HeroSelection(target);
        }
    }

    public void EndSection()
    {
        int heroCount = 0;

        if (GameObject.FindGameObjectWithTag("dialog") != null)
            return;

        Game game = Game.Get();
        foreach (Round.Hero h in game.round.heroes)
        {
            if (h.heroData != null) heroCount++;
        }

        if (heroCount < 2) return;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("heroselect"))
            Object.Destroy(go);

        for(int i = 0; i < game.round.heroes.Count - 1; i++)
        {
            int j = i;

            while(game.round.heroes[i].heroData == null && j < game.round.heroes.Count)
            {
                game.round.heroes[i].heroData = game.round.heroes[j].heroData;
                game.round.heroes[j].heroData = null;
                j++;
            }
        }

        UpdateImages();
        UpdateStatus();

        game.round.heroesSelected = true;
        game.moraleDisplay = new MoraleDisplay();
        // Create the menu button
        new MenuButton();

        EventHelper.EventTriggerType("EventStart");
    }
}