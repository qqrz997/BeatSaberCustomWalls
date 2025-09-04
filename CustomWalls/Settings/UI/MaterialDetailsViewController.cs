using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomWalls.Data;
using CustomWalls.Utilities;
using HMUI;
using TMPro;
using Zenject;

namespace CustomWalls.Settings.UI;

internal class MaterialDetailsViewController : BSMLResourceViewController
{
    [Inject] private readonly PluginConfig config = null!; 
    [Inject] private readonly MaterialAssetLoader materialAssetLoader = null!;    
    
    public override string ResourceName => "CustomWalls.Settings.UI.Views.materialDetails.bsml";

    private readonly string scoreDisabledByMaterial = "This CustomWall disables Score Submission";
    private readonly string scoreDisabledByUser = "Score Submission has been manually disabled";

    [UIComponent("material-description")]
    public TextPageScrollView materialDescription = null;

    [UIValue("enable-obstacle-frame")]
    public bool EnableObstacleFrame
    {
        get => config.EnableObstacleFrame;
        set => config.EnableObstacleFrame = value;
    }

    [UIComponent("score-submission-info")]
    public TextMeshProUGUI scoreSubmissionInfo = null;

    public void OnMaterialWasChanged(CustomMaterial customMaterial)
    {
        materialDescription.SetText($"{customMaterial.Descriptor.MaterialName}:\n\n{Utils.SafeUnescape(customMaterial.Descriptor.Description)}");
    }

    [UIAction("score-submission-manual-change")]
    public void OnManualScoreSubmissionChange(bool state)
    {
        if (state)
        {
            scoreSubmissionInfo.text = scoreDisabledByUser;
        }
        else
        {
            scoreSubmissionInfo.text = materialAssetLoader.GetSelectedMaterial().Descriptor.DisablesScore
                ? scoreDisabledByMaterial
                : string.Empty;
        }
    }
}