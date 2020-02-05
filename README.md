# JsonParser
Denormalize JsonObject to generate CSV Files

- Input
```json
@"{ 'name': 'Adler', 'secondName': 'Pagliarini',
    'obj1': { 'A': 1 },
    'obj2': { 'B': 2 },
    'objInArr': { 'B': 'B', 'C': { 'CC': 'CC', 'D': { 'DD': 'DDD' } }, InArr: [{ 'Arr': 'Arr_1' }, { 'Arr': 'Arr_2' }] },
    'ArrayN': [{ 'Array1': 1 }, { 'Array1': 11 }, { 'ArrayOfArray': [{ 'AofA': 'AofA_1' }, { 'AofA': 'AofA_2' }] }],
    'ArrayValues': [33, 99]
}";
```

- Output
```json
[{
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_Array1": 1,
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_Array1": 11,
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_ArrayOfArray_AofA": "AofA_1",
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_ArrayOfArray_AofA": "AofA_2",
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_Array1": 1,
    "ArrayValues": 99
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_Array1": 11,
    "ArrayValues": 99
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_ArrayOfArray_AofA": "AofA_1",
    "ArrayValues": 99
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_1",
    "ArrayN_ArrayOfArray_AofA": "AofA_2",
    "ArrayValues": 99
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_Array1": 1,
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_Array1": 11,
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_ArrayOfArray_AofA": "AofA_1",
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_ArrayOfArray_AofA": "AofA_2",
    "ArrayValues": 33
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_Array1": 1,
    "ArrayValues": 99
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_Array1": 11,
    "ArrayValues": 99
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_ArrayOfArray_AofA": "AofA_1",
    "ArrayValues": 99
}, {
    "name": "Adler",
    "secondName": "Pagliarini",
    "obj1_A": 1,
    "obj2_B": 2,
    "objInArr_B": "B",
    "objInArr_C_CC": "CC",
    "objInArr_C_D_DD": "DDD",
    "objInArr_InArr_Arr": "Arr_2",
    "ArrayN_ArrayOfArray_AofA": "AofA_2",
    "ArrayValues": 99
}]
```