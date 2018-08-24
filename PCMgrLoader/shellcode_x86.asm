.model flat
.data
assume fs:nothing
.code

?MGetPEBLdr@@YAPAXXZ PROC
mov ebx, 0
mov ebx, FS:[30h]
mov ebx, [ebx+0Ch]
mov eax, ebx
ret
?MGetPEBLdr@@YAPAXXZ ENDP

?MGetK32ModuleHandleCore@@YAPAXXZ PROC
xor edx, edx
mov edx, FS:[30h]
mov edx, [edx+dword ptr 0Ch]
mov edx, [edx+dword ptr 14h]  
next_mod:
mov esi, [edx+dword ptr 28h]
push 24
pop ecx
xor edi, edi
loop_modname:
xor eax, eax
lodsb
cmp al, 'a'
jl not_lowercase
sub al, 20
not_lowercase:
ror edi, 13
add edi, eax
loop loop_modname
cmp edi, 6A4ABC5Bh
mov ebx, [edx+dword ptr 10h]
mov edx, [edx]
jne next_mod
mov eax, ebx
retn 8
?MGetK32ModuleHandleCore@@YAPAXXZ ENDP

END