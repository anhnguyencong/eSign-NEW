export class JobModel {
    id!: string;
    refId!: string;
    batchId!: string;
    sourceName!: string;
    documentName!: string;
    refNumber!: string;
    appTokenKey!: string;
    status!: string;
    callbackStatus!: string;
    createdDate!: Date;
    priority!: number;
    completedFileName!:string;
    completedFileUrl!:string;
    note!:string;

    constructor(id: string, refId: string, batchId: string, sourceName: string,
        documentName: string, refNumber: string, status: string, createdDate: Date, priority: number) {
        this.id = id;
        this.refId = refId;
        this.batchId = batchId;
        this.sourceName = sourceName;
        this.documentName = documentName;
        this.refNumber = refNumber;
        this.status = status
        this.createdDate = createdDate;
        this.priority = priority;
    }
}

export class CompletedFileModel{
    fileName!:string;
    fileUrl!:string;
}