import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ClipboardService } from 'ngx-clipboard';
import { SelectItem, LazyLoadEvent, MessageService } from 'primeng/api';
import { Dropdown } from 'primeng/dropdown';
import { JobModel } from 'src/app/models/jobs/job.model';
import { EJobCallbackStatus, EJobStatus } from '../../../_enums/jobstatus.enum';
import { JobFilterModel } from '../../../models/jobs/jobfilter.model';
import { JobService } from '../../../_services/job.service'
import { FileService } from 'src/app/_services/file.service';
import { $enum } from "ts-enum-util";

import { Pipe } from '@angular/core';
import { JobErrorModel } from 'src/app/models/jobs/jobError.model';


Pipe({
  name: 'eNumAsString'
})


export enum PriorityTypeSubmit {
  BatchId,
  Job
}

interface JobStatus {
  name: string,
  code: string
}

class Paging {
  pageIndex: number = 1;
  pageSize: number = 0;
  pageDisplay: number = 10;
  totalRows: number = 0;
}

class JobResult {
  toTal: number = 0;
  completed: number = 0;
  inProgress: number = 0;
  pending: number = 0;
  failed: number = 0;
  callbackCompleted: number = 0;
  callbackPending: number = 0;
  callbackFailed: number = 0;
}

class JobPriority {
  updateType: PriorityTypeSubmit = PriorityTypeSubmit.Job;
  id!: string;
  priority!: number;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],

})

export class DashboardComponent implements OnInit {
  jobResult!: JobResult;
  jobPaging!: Paging;
  jobs: JobModel[] = [];
  selectedJob!: JobModel;
  sourceNames: SelectItem[] = [];
  jobStatuses: SelectItem[] = [];
  jobStatuses1: any[] = [];
  batchIds: SelectItem[] = [];
  result: any;
  jobFilter: JobFilterModel = new JobFilterModel();
  jobFilterForm!: FormGroup;
  jobPriority: JobPriority = new JobPriority();
  priorityTypeSubmit = PriorityTypeSubmit;
  priorityForm!: FormGroup;
  isFilterFormValid: boolean = false;
  isSetPriority: boolean = false;
  isRetryAllSubmit: boolean = false;
  isRetryCallbackSubmit: boolean = false;
  totalRecords: number = 0;
  message: string = '';
  displayBasic: boolean = false;
  valuePriority: number = 0;
  currentPage: number = 1;
  EJobStatus = EJobStatus;
  first = 0;
  rows = 10;
  batchIdCurrent!: string;
  displayPopupError: boolean = false;
  jobError: JobErrorModel = new JobErrorModel();
  loading: boolean = false;
  scrollableCols!: any[];
  frozenCols!: any[];

  get f() { return this.jobFilterForm.controls; }

  constructor(private jobService: JobService,
    private fileService: FileService,
    private formBuilder: FormBuilder,
    private _clipboardService: ClipboardService,
    private messageService: MessageService) { }

  ngOnInit(): void {
    this.getSourceName();
    this.getBatchIds('');
    this.jobStatuses = Object.keys(EJobStatus).map((key, value) => ({ label: EJobStatus[key as keyof typeof EJobStatus].toString(), value: key }));

    this.refreshList();
    this.jobResult = new JobResult();
    this.jobResult.toTal = 0;
    this.jobResult.completed = 0;
    this.jobResult.inProgress = 0;
    this.jobResult.pending = 0;
    this.jobResult.failed = 0;
    this.jobPaging = new Paging();
    this.jobPaging.pageDisplay = 10;
    this.jobPaging.totalRows = 0;
    this.jobPaging.pageIndex = 0;
    this.jobFilterForm = this.formBuilder.group({
      sourceName: [this.jobFilter.sourceName],
      textSearch: [this.jobFilter.keyword = '', [Validators.maxLength(255)]],
      batchId: [this.jobFilter.batchId],
      status: [this.jobFilter.status]
    });
    this.priorityForm = this.formBuilder.group({
      updateType: this.jobPriority.updateType,
      id: [this.jobPriority.id, [Validators.required]],
      priority: [this.jobPriority.priority, [Validators.required, Validators.min(1), Validators.max(10)]]
    })

    this.frozenCols = [
      { field: 'refId', header: 'Reference ID' }
  ];

  this.scrollableCols = [
      { field: 'batchId', header: 'Batch Id' },
      { field: 'sourceName', header: 'Source name' },
      { field: 'documentName', header: 'Document name' },
      { field: 'refNumber', header: 'Ref Policy number' },
      { field: 'status', header: 'Status' },
      { field: 'callbackStatus', header: 'Callback Status' },
      { field: 'createdDate', header: 'Created Date' },
      { field: 'priority', header: 'Priority' },
      { field: 'action', header: '' }
  ];
  }

  setPriority(priorityType: PriorityTypeSubmit, id: string, val: number) {
    this.jobPriority.updateType = priorityType;
    this.jobPriority.priority = val;
    if (priorityType == PriorityTypeSubmit.BatchId) {
      this.jobPriority.id = this.jobFilter.batchId;
    }
    else {
      this.jobPriority.id = id;
    }
  }

  refreshList() {
    this.loading = true;
    var query = '?PageIndex=' + this.currentPage + '&PageSize=10';
    var filterStr = "";
    this.batchIdCurrent = "";
    if (this.jobFilterForm) {
      if (this.jobFilterForm.value.textSearch) {
        filterStr = filterStr + '&TextSearch=' + this.jobFilterForm.value.textSearch;
      }
      if (this.jobFilterForm.value.sourceName) {
        filterStr = filterStr + '&SourceName=' + this.jobFilterForm.value.sourceName;
      }
      if (this.jobFilterForm.value.batchId) {
        this.jobFilter.batchId = this.jobFilterForm.value.batchId;
        filterStr = filterStr + '&BatchId=' + this.jobFilterForm.value.batchId;
        this.batchIdCurrent = this.jobFilterForm.value.batchId;
      }
      if (this.jobFilterForm.value.status) {
        var jobStatus = this.jobFilterForm.value.status;
        var jobStatusValue = $enum(EJobStatus).getValueOrThrow(jobStatus);
        if (jobStatusValue == EJobStatus.Pending || jobStatusValue == EJobStatus.Processing ||
          jobStatusValue == EJobStatus.Completed || jobStatusValue == EJobStatus.Failed) {
          filterStr = filterStr + '&StatusIds=' + jobStatus;
        }
        else {
          filterStr = filterStr + '&CallbackStatusIds=' + jobStatus.toString().replace('Callback', '');
        }
      }
    }
    this.jobService.getList(query + filterStr).toPromise().then(data => {
      if (data.success) {
        this.jobs = data.result.items.items;
        this.jobPaging.totalRows = data.result.items.totalCount;

        //set summary
        this.jobResult.toTal = data.result.jobSummary.total;
        this.jobResult.completed = data.result.jobSummary.completed;
        this.jobResult.inProgress = data.result.jobSummary.inProgress;
        this.jobResult.pending = data.result.jobSummary.pending;
        this.jobResult.failed = data.result.jobSummary.failed;
        this.jobResult.callbackCompleted = data.result.jobSummary.callbackComplete;
        this.jobResult.callbackPending = data.result.jobSummary.callbackPending;
        this.jobResult.callbackFailed = data.result.jobSummary.callbackFailed;

        this.setStatusButton();

      }

      this.loading = false;
    });
  }

  OnChangeSourceName(ev: any) {
    console.log(ev);
    this.jobFilter.batchId = '';
    this.jobFilterForm.reset({
      sourceName: this.jobFilterForm.value.sourceName,
      batchId: '',
      status: this.jobFilterForm.value.status,
      textSearch: this.jobFilterForm.value.textSearch

    });
    this.getBatchIds(ev.value);
  }

  OnChangeBatchId(ev: any) {
  }

  OnChangeStatus(ev: any) {

  }

  onSubmit() {
    this.first = 0;
    this.currentPage = 1;
    this.refreshList();
  }

  onSubmitPriority() {
    this.jobService.setPriority(this.priorityForm.value).subscribe(res => {
      if (res.success) {
        this.refreshList();
      }
      else {
        //Show toast error
      }
    }, (e) => {
      console.log(e);
    });
  }

  loadJobs(event: LazyLoadEvent) {
    var first = event.first!;
    var pageSize = event.rows! == 0 ? 1 : event.rows!;
    var pageIndex = first / pageSize + 1;
    this.currentPage = pageIndex;
    this.refreshList();

  }

  getSourceName() {
    this.jobService.getList('/SourceName').toPromise().then(data => {
      if (data.success) {
        this.sourceNames = [];
        for (let item of data.result) {
          this.sourceNames.push({ label: item['value'], value: item['key'] });
        }
      }
    });
  }

  getBatchIds(sourceName: string) {
    var query = '';
    if (sourceName) {
      this.batchIds = [];
      query = '?AppName=' + sourceName;
      this.jobService.getList('/BatchIds' + query).toPromise().then(data => {
        if (data.success) {

          for (let item of data.result) {
            this.batchIds.push({ label: item['value'], value: item['key'] });
          }
        }
      });
    }
    else {
      this.batchIds = [];
    }

  }

  retry(id: string) {
    this.jobService.retryJob(id).subscribe(res => {
      if (res.success) {
        this.refreshList();
      }
      else {
        //Show toast error
      }
    }, (e) => {
      console.log(e);
    });
  }

  retryByBatchId() {
    this.jobService.retryByBatchId(this.batchIdCurrent).subscribe(res => {
      if (res.success) {
        this.refreshList();
      }
      else {
        //Show toast error
      }
    }, (e) => {
      console.log(e);
    });
  }

  retryCallbackByBatchId() {
    this.jobService.retryCallbackByBatchId(this.batchIdCurrent).subscribe(res => {
      if (res.success) {
        this.refreshList();
      }
      else {
        //Show toast error
      }
    }, (e) => {
      console.log(e);
    });
  }

  retryCallback(id: string) {
    this.jobService.retryCallback(id).subscribe(res => {
      if (res.success) {
        this.refreshList();
      }
      else {
        //Show toast error
      }
    }, (e) => {
      console.log(e);
    });
  }

  downloadFile(idJob: string) {
    var job = this.jobs.filter(x => x.id === idJob)
    if (job.length > 0) {
      var jobSelected = job[0];
      console.log(jobSelected);
      if (jobSelected.completedFileUrl && jobSelected.completedFileName) {
        this.fileService.download(jobSelected.completedFileUrl, jobSelected.appTokenKey).subscribe(respData => {
          var blob = new Blob([respData]);
          var url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = jobSelected.completedFileName;
          link.target = '_blank';
          link.click();
        }, error => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.statusText });
        });
      }
      else{
        this.fileService.download(jobSelected.completedFileUrl, jobSelected.appTokenKey).subscribe(respData => {
          var blob = new Blob([respData]);
          var url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = jobSelected.completedFileName;
          link.target = '_blank';
          link.click();
        }, error => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.statusText });
        });
      }
    }

  }

  getValueEnumStatus(key: string): string {
    if (key) {
      return $enum(EJobStatus).getValueOrThrow(key);
    }
    return '';
  }

  getValueEnumCallbackStatus(key: string): string {
    if (key) {
      console.log(key);
      return $enum(EJobCallbackStatus).getValueOrThrow(key);
    }
    return '';
  }

  setStatusButton() {
    this.isSetPriority = (this.batchIdCurrent && this.jobFilter.status == 'Pending') || false;
    this.isRetryAllSubmit = (this.batchIdCurrent && this.jobFilter.status == 'Failed') || false;
    this.isRetryCallbackSubmit = (this.batchIdCurrent && this.jobFilter.status == 'CallbackFailed') || false;
    console.log(this.jobFilter.status);
  }

  clearFilter(dropdown: Dropdown) {
    this.jobFilterForm.value.batchId = '';
    dropdown.resetFilter();
  }

  showError(idJob: string) {
    var job = this.jobs.filter(x => x.id === idJob);
    if (job.length > 0) {
      var jobSelected = job[0];
      var arrError: string[] = [];
      for (let i = 0; i < 5; i++) {
        arrError.push(i.toString() + " - error");
      }
      if (jobSelected) {
        this.jobError.header = jobSelected.refId;
        this.jobError.content = jobSelected.note;
      } else {
        this.jobError.header = '';
        this.jobError.content = '';
      }

    }
    this.displayPopupError = true;
  }
}

