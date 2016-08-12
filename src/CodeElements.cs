public class CodeElement {
	public override string ToString() { return this.GetType().Name; }
}
public class DataDivisionHeader: CodeElement { }
public abstract class DataSectionHeader: CodeElement { }
public class WorkingStorageSectionHeader: DataSectionHeader { }
public class LocalStorageSectionHeader: DataSectionHeader { }
public class LinkageSectionHeader: DataSectionHeader { }
public abstract class DataDefinitionEntry: CodeElement {
	public string Name { get; set; }
	public int Level { get; set; }
	public DataDefinitionEntry() { Name = "?"; Level =  1; }
	public override string ToString() { return string.Format("{0:00}",Level)+' '+Name; }
}
public class DataDescriptionEntry: DataDefinitionEntry {
	public string Picture { get; set; }
}
public class DataConditionEntry: DataDefinitionEntry { }
public class TypeDescriptionEntry: DataDefinitionEntry {
	public int Size { get; set; }
}
