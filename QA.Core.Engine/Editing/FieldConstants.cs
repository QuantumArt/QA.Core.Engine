
namespace QA.Core.Engine.Editing
{
    /// <summary>
    /// Получение имя поля QP по названию
    /// </summary>
    public class AbstractItemFieldNameResolver : IAbstractItemFieldNameResolver
    {
        #region Constants
        private const string Field_Name = "field_1145";
        private const string Field_Parent = "field_1147";
        private const string Field_IsPage = "field_1149";
        private const string Field_ZoneName = "field_1150";
        private const string Field_Discriminator = "field_1156";
        private const string Field_VersionOf = "field_1159";
        private const string Field_ExtensionId = "field_1158";
        private const string Field_IsInSiteMap = "field_1172"; 
        #endregion

        /// <summary>
        /// Получение имя поля QP по названию
        /// <param name="name">Название поля</param>
        /// <returns>Имя поля</returns>
        /// </summary>
        public string Resolve(string name)
        {
            // TODO: реализовать способ получения названий полей контента
            // а сейчас просто укажем в коде.

            switch (name)
            {
                case "Name": return Field_Name;
                case "Parent": return Field_Parent;
                case "IsPage": return Field_IsPage;
                case "ZoneName": return  Field_ZoneName;
                case "Discriminator": return  Field_Discriminator;
                case "VersionOf": return  Field_VersionOf;
                case "ExtensionId": return  Field_ExtensionId;
                case "IsInSiteMap": return Field_IsInSiteMap;

                default:
                    break;
            }

            return null;
        }
    }
}
