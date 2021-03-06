﻿using System;
using UnityEngine;

public class MapStructure : MapObject, IDynlight, IPlayerPawn, IVulnerable, IDisposable
{
    public override MapObjectType GetObjectType() { return MapObjectType.Structure; }
    protected override Type GetGameObjectType() { return typeof(MapViewStructure); }

    public StructureClass Class = null;
    public Templates.TplStructure Template = null;
    public int CurrentFrame = 0;
    public int CurrentTime = 0;
    public int HealthMax = 0;
    private int _Health = 0;
    public int Health
    {
        get
        {
            return _Health;
        }

        set
        {
            if (value > HealthMax)
                value = HealthMax;
            if (value < 0)
                value = 0;
            _Health = value;
        }
    }
    private Player _Player;

    public Player Player
    {
        get
        {
            return _Player;
        }

        set
        {
            if (_Player != null)
                _Player.Objects.Remove(this);
            _Player = value;
            _Player.Objects.Add(this);
        }
    }

    public Player GetPlayer() { return _Player; }
    public bool IsBridge = false;
    public int Tag = 0;
    public float ScanRange = 0;

    // this is used for dynlight
    private int LightTime = 0;
    private int LightFrame = 0;
    private int LightValue = 0; // basically, this gets set if structure is a dynlight
    public int GetLightValue() { return LightValue; }

    public MapStructure(int typeId)
    {
        Class = StructureClassLoader.GetStructureClassById(typeId);
        if (Class == null)
            Debug.LogFormat("Invalid structure created (typeId={0})", typeId);
        else InitStructure();
    }

    public MapStructure(string name)
    {
        Class = StructureClassLoader.GetStructureClassByName(name);
        if (Class == null)
            Debug.LogFormat("Invalid structure created (name={0})", name);
        else InitStructure();
    }

    public override void Dispose()
    {
        base.Dispose();
        if (_Player != null)
            _Player.Objects.Remove(this);
    }

    private void InitStructure()
    {
        Template = TemplateLoader.GetStructureById(Class.ID);
        if (Template == null)
        {
            Debug.LogFormat("Invalid structure created (template not found, typeId={0})", Class.ID);
            Class = null;
            return;
        }

        HealthMax = Health = Template.HealthMax;
        Width = Template.Width;
        Height = Template.Height;
        ScanRange = Template.ScanRange; // only default scanrange
        DoUpdateView = true;
    }

    public override void Update()
    {
        if (Class == null)
            return;

        UpdateNetVisibility();

        // perform animation
        // do not animate if visibility != 2, also do not render at all if visibility == 0
        if (Class.Frames.Length > 1 && GetVisibility() == 2)
        {
            CurrentTime++;
            if (CurrentTime > Class.Frames[CurrentFrame].Time)
            {
                CurrentFrame = ++CurrentFrame % Class.Frames.Length;
                CurrentTime = 0;
                DoUpdateView = true;
            }

            if (Class.LightRadius > 0)
            {
                LightTime++;
                if (LightTime > Class.LightPulse)
                {
                    LightFrame++;
                    LightValue = (int)(Mathf.Sin((float)LightFrame / 3) * 64 + 128);
                    MapLogic.Instance.CalculateDynLighting();
                }
            }
        }
    }

    public override MapNodeFlags GetNodeLinkFlags(int x, int y)
    {
        if (IsBridge) return MapNodeFlags.Unblocked;

        bool canNotPass = ((1 << (y * Width + x)) & Template.CanNotPass) != 0;
        bool canPass = ((1 << (y * Width + x)) & Template.CanPass) != 0;
        if (!canPass) return MapNodeFlags.Unblocked;
        if (canNotPass) return MapNodeFlags.DynamicGround;
        return 0;
    }

    public int TakeDamage(DamageFlags flags, MapUnit source, int count)
    {
        if ((flags & DamageFlags.TerrainDamage) == 0)
            return 0;

        Health -= count;
        return count;
    }
}