using System.ComponentModel;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TrackGenerator trackGenerator;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private ShopUI shopUI;

    public override void InstallBindings()
    {
        Container.Bind<ISaveService>().To<SaveService>().AsSingle();
        Container.Bind<MetaProgress>().AsSingle();

        Container.Bind<GameManager>().FromInstance(gameManager).AsSingle();
        Container.Bind<PlayerController>().FromInstance(playerController).AsSingle();
        Container.Bind<TrackGenerator>().FromInstance(trackGenerator).AsSingle();
        Container.Bind<UIManager>().FromInstance(uiManager).AsSingle();
        Container.Bind<MusicManager>().FromInstance(musicManager).AsSingle();
        Container.Bind<ShopUI>().FromInstance(shopUI).AsSingle();
    }
}