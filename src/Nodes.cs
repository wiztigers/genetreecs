
public class DataDivision: Node, CodeElementHolder<DataDivisionHeader> {
	public DataDivision(DataDivisionHeader header): base(header) { }
}

public abstract class DataSection: Node, CodeElementHolder<DataSectionHeader>, Child<DataDivision>, Parent<DataDefinition> {
	public DataSection(DataSectionHeader header): base(header) { }
	public virtual bool IsShared { get { return false; } }
}
public class WorkingStorageSection: DataSection, CodeElementHolder<WorkingStorageSectionHeader> {
	public WorkingStorageSection(WorkingStorageSectionHeader header): base(header) { }
	public override string ID { get { return "working"; } }
}
public class LocalStorageSection: DataSection, CodeElementHolder<LocalStorageSectionHeader> {
	public LocalStorageSection(LocalStorageSectionHeader header): base(header) { }
	public override string ID { get { return "local"; } }
}
public class LinkageSection: DataSection, CodeElementHolder<LinkageSectionHeader> {
	public LinkageSection(LinkageSectionHeader header): base(header) { }
	public override string ID { get { return "linkage"; } }
	public override bool IsShared { get { return true; } }
}

public abstract class DataDefinition: Node, CodeElementHolder<DataDefinitionEntry>, Child<DataSection> {
	public DataDefinition(DataDefinitionEntry entry): base(entry) { }
	public override string ID { get { return this.CodeElement().Name; } }
	public string Name { get { return this.CodeElement().Name; } }
}
public class DataDescription: DataDefinition, CodeElementHolder<DataDescriptionEntry>, Parent<DataDescription> {
	public DataDescription(DataDescriptionEntry entry): base(entry) { }
}
public class DataCondition: DataDefinition, CodeElementHolder<DataConditionEntry> {
	public DataCondition(DataConditionEntry entry): base(entry) { }
}
public class TypeDescription: DataDefinition, CodeElementHolder<TypeDefinitionEntry>, Parent<DataDescription> {
	public TypeDescription(TypeDefinitionEntry entry): base(entry) { }
	public bool IsStrong { get; internal set; }
}
