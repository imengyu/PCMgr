.model flat
.data
assume fs:nothing
.code

_MGetCurrentPeb PROC
mov ebx, 0
mov ebx, FS:[30h]
mov eax, ebx
ret
_MGetCurrentPeb ENDP

END