export enum EJobStatus {
  Pending = "Pending",
  Processing = "In Progress",
  Completed = "Completed",
  Failed = "Failed",
  CallbackPending = "Callback Pending",
  CallbackCompleted = "Callback Completed",
  CallbackFailed = "Callback Failed"
}

export enum EJobCallbackStatus {
  Pending = "Pending",
  Completed = "Completed",
  Failed = "Failed"
}