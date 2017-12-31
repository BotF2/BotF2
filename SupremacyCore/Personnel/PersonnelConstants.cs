using System;

using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;
using Supremacy.Xaml;
using Supremacy.VFS;

namespace Supremacy.Personnel
{
    [Serializable]
    public class PersonnelConstants : SupportInitializeBase
    {
        private const string DataFileUri = "vfs:///Resources/Data/PersonnelConstants.xaml";

        private int _trainingDuration = 2;
        private int _naturalSkillsPerAgent = 3;
        private int _maxActiveAgentsPerEmpire = 8;
        private int _minTurnsBetweenAgentRecruitment = 2;

        private static PersonnelConstants _instance;

        public static PersonnelConstants Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PersonnelConstants();
                    _instance.Refresh();
                }
                return _instance;
            }
        }

        public int NaturalSkillsPerAgent
        {
            get { return _naturalSkillsPerAgent; }
            set
            {
                VerifyInitializing();
                _naturalSkillsPerAgent = value;
            }
        }

        public int MaxActiveAgentsPerEmpire
        {
            get { return _maxActiveAgentsPerEmpire; }
            set
            {
                VerifyInitializing();
                _maxActiveAgentsPerEmpire = value;
            }
        }

        public int MinTurnsBetweenAgentRecruitment
        {
            get { return _minTurnsBetweenAgentRecruitment; }
            set
            {
                VerifyInitializing();
                _minTurnsBetweenAgentRecruitment = value;
            }
        }

        public int TrainingDuration
        {
            get { return _trainingDuration; }
            set
            {
                VerifyInitializing();
                _trainingDuration = value;
            }
        }

        public void Refresh()
        {
            try
            {
                IVirtualFileInfo dataFile;
                
                if (!ResourceManager.VfsService.TryGetFileInfo(new Uri(DataFileUri), out dataFile) ||
                    !dataFile.Exists)
                {
                    GameLog.Client.GameData.WarnFormat(
                        "Could not locate data file \"{0}\".  Using default values instead.",
                        DataFileUri);
                    
                    return;
                }

                using (var stream = dataFile.OpenRead())
                {
                    XamlHelper.LoadInto(this, stream);
                }
            }
            catch (Exception e)
            {
                GameLog.Client.GameData.Error(
                    string.Format(
                        "An error occurred while loading data file \"{0}\".  " +
                        "Check the error log for exception details.",
                        DataFileUri),
                    e);
            }
        }

        protected override void EndInitCore()
        {
            Validate();
        }

        private void Validate()
        {
            var totalAgentSkills = EnumHelper.GetValues<AgentSkill>().Length;

            if (_naturalSkillsPerAgent < 0)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Personnel constant '{0}' has a negative value.  " +
                    "Using the minimum value of '0' instead.",
                    "NaturalSkillsPerAgent");

                _naturalSkillsPerAgent = 0;
            }
            else if (_naturalSkillsPerAgent > totalAgentSkills)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Personnel constant '{0}' is too large.  " +
                    "Using the maximum value of '{1}' instead.",
                    "NaturalSkillsPerAgent",
                    totalAgentSkills);

                _naturalSkillsPerAgent = totalAgentSkills;
            }

            if (_maxActiveAgentsPerEmpire < 0)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Personnel constant '{0}' has a negative value.  " +
                    "Using the minimum value of '0' instead.  This will " +
                    "effectively disable the personnel system.",
                    "MaxActiveAgentsPerEmpire");

                _maxActiveAgentsPerEmpire = 0;
            }
            else if (_maxActiveAgentsPerEmpire == 0)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Personnel constant '{0}' is set to '0'.  " +
                    "This will effectively disable the personnel system.",
                    "MaxActiveAgentsPerEmpire");
            }

            if (_minTurnsBetweenAgentRecruitment < 0)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Personnel constant '{0}' has a negative value.  " +
                    "Using the minimum value of '0' instead.",
                    "MinTurnsBetweenAgentRecruitment");

                _minTurnsBetweenAgentRecruitment = 0;
            }

            if (_trainingDuration <= 0)
            {
                GameLog.Client.GameData.WarnFormat(
                    "Personnel constant '{0}' has a non-positive value.  " +
                    "Using the minimum value of '1' instead.",
                    "TrainingDuration");

                _trainingDuration = 1;
            }
        }
    }
}