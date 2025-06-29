using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameInstaller : MonoInstaller
{

    public override void InstallBindings()
    {
        Container.BindInstance(GetComponent<SceneController>()).AsSingle();
        Container.BindInstance(GetComponent<BattleField>()).AsSingle();
        Container.BindInstance(new State()).AsSingle();

    }
}