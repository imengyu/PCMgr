
.data
extern PsGetNextProcessThread:qword

.code


KxGetNextProcessThread_x64Call PROC
mov [rsp-8+qword ptr 10h], rdx
mov [rsp-8+qword ptr 18h], rcx
push rbp
push rdi
mov rcx, rbp
call PsGetNextProcessThread
pop rdi
pop rbp
ret
KxGetNextProcessThread_x64Call ENDP

END