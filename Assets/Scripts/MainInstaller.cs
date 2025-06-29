using System;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    
    public SceneController _sceneController;
    public override void InstallBindings()
    {
        Container.Bind<SceneController>().FromInstance(_sceneController).AsSingle();
    }
}