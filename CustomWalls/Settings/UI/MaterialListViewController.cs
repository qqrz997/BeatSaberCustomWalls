﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomWalls.Data;
using CustomWalls.Utilities;
using HMUI;
using System;
using System.Linq;
using UnityEngine;

namespace CustomWalls.Settings.UI
{
    internal class MaterialListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "CustomWalls.Settings.UI.Views.materialList.bsml";

        private bool isGeneratingPreview = false;
        private GameObject preview;
        private ColorSchemesSettings currentColorScheme = null;

        // Objects
        private GameObject materialObject;

        public Action<CustomMaterial> customMaterialChanged;

        [UIComponent("materialList")]
        public CustomListTableData customListTableData = null;


        [UIAction("materialSelect")]
        public void Select(TableView _, int row)
        {
            MaterialAssetLoader.SelectedMaterial = row;
            Configuration.CurrentlySelectedMaterial = MaterialAssetLoader.CustomMaterialObjects[row].FileName;

            GenerateMaterialPreview(row);
        }

        [UIAction("reloadMaterials")]
        public async void ReloadMaterials()
        {
            await MaterialAssetLoader.Reload();
            SetupList();
            Select(customListTableData.TableView, MaterialAssetLoader.SelectedMaterial);
        }

        [UIAction("#post-parse")]
        public void SetupList()
        {
            customListTableData.Data.Clear();
            foreach (CustomMaterial material in MaterialAssetLoader.CustomMaterialObjects)
            {
                Sprite sprite = material?.Descriptor?.Icon
                    ? Sprite.Create(material.Descriptor.Icon, new Rect(Vector2.zero, new Vector2(material.Descriptor.Icon.width, material.Descriptor.Icon.height)), new Vector2(0.5f, 0.5f))
                    : null;
                CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(material.Descriptor.MaterialName, material.Descriptor.AuthorName, sprite);
                customListTableData.Data.Add(customCellInfo);
            }

            customListTableData.TableView.ReloadData();
            int selectedMaterial = MaterialAssetLoader.SelectedMaterial;

            customListTableData.TableView.ScrollToCellWithIdx(selectedMaterial, TableView.ScrollPositionType.Beginning, false);
            customListTableData.TableView.SelectCellWithIdx(selectedMaterial);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            currentColorScheme = Resources.FindObjectsOfTypeAll<GameplaySetupViewController>().LastOrDefault().colorSchemesSettings;

            if (!preview)
            {
                preview = new GameObject();
                preview.transform.position = new Vector3(3.30f, 1.25f, 1.70f);
                preview.transform.Rotate(0.0f, -22.5f, 0.0f);
            }

            Select(customListTableData.TableView, MaterialAssetLoader.SelectedMaterial);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            ClearPreview();
        }

        private void GenerateMaterialPreview(int selectedMaterial)
        {
            if (!isGeneratingPreview)
            {
                try
                {
                    isGeneratingPreview = true;
                    ClearObjects();

                    CustomMaterial currentMaterial = MaterialAssetLoader.CustomMaterialObjects[selectedMaterial];
                    if (currentMaterial != null)
                    {
                        customMaterialChanged?.Invoke(currentMaterial);
                        InitializePreviewObjects(currentMaterial, preview.transform);
                    }
                }
                catch (Exception ex)
                {
                    Logger.log.Error(ex);
                }
                finally
                {
                    isGeneratingPreview = false;
                }
            }
        }

        private void InitializePreviewObjects(CustomMaterial customMaterial, Transform transform)
        {
            if (customMaterial.GameObject)
            {
                materialObject = CreatePreviewMaterial(customMaterial.GameObject, transform);

                if (materialObject)
                {
                    // Fix irregular model scales (in most cases)
                    foreach (Transform child in materialObject.GetComponentsInChildren<Transform>(false))
                    {
                        if (child)
                        {
                            child.localScale = Vector3.one;
                            child.localPosition = Vector3.zero;
                        }
                    }

                    materialObject.transform.localScale = new Vector3(15f, 50.0f, 100f);
                    if (customMaterial.Descriptor.ReplaceMesh)
                    {
                        // Account for custom mesh scale being weird in previews
                        materialObject.transform.localScale *= 0.025f;
                    }

                    Renderer renderer = materialObject.gameObject?.GetComponentInChildren<Renderer>();
                    MaterialUtils.SetMaterialsColor(renderer?.materials, currentColorScheme.GetSelectedColorScheme().obstaclesColor);
                }
            }
        }

        private GameObject CreatePreviewMaterial(GameObject material, Transform transform)
        {
            GameObject gameObject = InstantiateGameObject(material, transform);
            return gameObject;
        }

        private GameObject InstantiateGameObject(GameObject gameObject, Transform transform = null)
        {
            if (gameObject)
            {
                return transform ? Instantiate(gameObject, transform) : Instantiate(gameObject);
            }

            return null;
        }

        private void ClearPreview()
        {
            ClearObjects();
            DestroyGameObject(ref preview);
        }

        private void ClearObjects()
        {
            DestroyGameObject(ref materialObject);
        }

        private void DestroyGameObject(ref GameObject gameObject)
        {
            if (gameObject)
            {
                Destroy(gameObject);
                gameObject = null;
            }
        }
    }
}
