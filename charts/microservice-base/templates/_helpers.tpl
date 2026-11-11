{{/* Shared snippets used by deployment.yaml, service.yaml and migrate-job.yaml. Not deployed itself. */}}

{{- define "microservice.selectorLabels" -}}
app.kubernetes.io/name: {{ .Release.Name }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "microservice.labels" -}}
{{ include "microservice.selectorLabels" . }}
app.kubernetes.io/part-of: aegisflow
app.kubernetes.io/managed-by: {{ .Release.Service }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version }}
{{- end }}

{{- define "microservice.image" -}}
{{ required "image.repository is required" .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}
{{- end }}

{{/* Database settings, identical for the API and its migration Job. */}}
{{- define "microservice.env" -}}
- name: DB_SERVER
  value: {{ .Values.database.server | quote }}
- name: DB_NAME
  value: {{ required "database.name is required" .Values.database.name | quote }}
- name: DB_USER
  value: {{ .Values.database.user | quote }}
- name: DB_TRUST_SERVER_CERTIFICATE
  value: {{ .Values.database.trustServerCertificate | quote }}
- name: DB_PASSWORD
  valueFrom:
    secretKeyRef:
      name: {{ .Values.database.passwordSecret.name }}
      key: {{ .Values.database.passwordSecret.key }}
{{- end }}

{{/* Non-root, read-only filesystem, no extra Linux capabilities. */}}
{{- define "microservice.containerSecurity" -}}
allowPrivilegeEscalation: false
readOnlyRootFilesystem: true
capabilities:
  drop: ["ALL"]
{{- end }}
