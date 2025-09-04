using CustomWalls.Data;
using SiraUtil.Submissions;
using Zenject;

namespace CustomWalls;

public class SubmissionsManager : IInitializable
{
    private readonly CustomMaterial customMaterial;
    private readonly Submission submission;

    public SubmissionsManager(CustomMaterial customMaterial, Submission submission)
    {
        this.customMaterial = customMaterial;
        this.submission = submission;
    }

    public void Initialize()
    {
        if (customMaterial.Descriptor.DisablesScore)
        {
            submission.DisableScoreSubmission("CustomWalls", "Selected Custom Wall Disables Scoring");
        }
    }
}