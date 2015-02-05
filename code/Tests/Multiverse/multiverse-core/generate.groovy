import org.apache.velocity.Template
import org.apache.velocity.VelocityContext
import org.apache.velocity.app.VelocityEngine

@Grab(group = 'org.apache.velocity', module = 'velocity', version = '1.6.4')

class TxnCallable {
    String type
    String name
    String typeParameter
}

class TxnExecutor {
    String name
    boolean lean
}

class TxnObject {
    String type//the type of data it contains
    String objectType//the type of data it contains
    String initialValue//the initial value
    String typeParameter
//  String parametrizedTranlocal
    String functionClass//the class of the callable used for commuting operations
    String incFunctionMethod
    boolean isReference
    String referenceInterface
    boolean isNumber
    String predicateClass
}

VelocityEngine engine = new VelocityEngine();
engine.init();

def refs = createTxnObjects();
def txnCallables = createCallables();TxnCallable
def txnExecutors = [new TxnExecutor(name: 'FatGammaTxnExecutor', lean: false),
        new TxnExecutor(name: 'LeanGammaTxnExecutor', lean: true)]

generateRefFactory(engine, refs);

for (def param in refs) {
    generateRefs(engine, param)
    generatePredicate(engine, param)
    generateFunction(engine, param)
}

for (def closure in txnCallables) {
    generateTxnCallable(engine, closure)
}

generateTxnExecutor(engine, txnCallables)
//generateOrElseBlock(engine, atomicCallables)
generateGammaOrElseBlock(engine, txnCallables)
generateStmUtils(engine, txnCallables)

for (def txnExecutor in txnExecutors) {
    generateGammaTxnExecutor(engine, txnExecutor, txnCallables)
}


List<TxnCallable> createCallables() {
    def result = []
    result << new TxnCallable(
            name: 'TxnCallable',
            type: 'E',
            typeParameter: '<E>'
    )
    result << new TxnCallable(
            name: 'TxnIntCallable',
            type: 'int',
            typeParameter: ''
    )
    result << new TxnCallable(
            name: 'TxnLongCallable',
            type: 'long',
            typeParameter: ''
    )
    result << new TxnCallable(
            name: 'TxnDoubleCallable',
            type: 'double',
            typeParameter: ''
    )
    result << new TxnCallable(
            name: 'TxnBooleanCallable',
            type: 'boolean',
            typeParameter: ''
    )
    result << new TxnCallable(
            name: 'TxnVoidCallable',
            type: 'void',
            typeParameter: ''
    )
    result
}

List<TxnObject> createTxnObjects() {
    def result = []
    result.add new TxnObject(
            type: 'E',
            objectType: '',
            typeParameter: '<E>',
            initialValue: 'null',
            referenceInterface: 'TxnRef',
            functionClass: 'Function',
            isReference: true,
            isNumber: false,
            predicateClass: "Predicate",
            incFunctionMethod: '')
    result.add new TxnObject(
            type: 'int',
            objectType: 'Integer',
            referenceInterface: 'TxnInteger',
            typeParameter: '',
            initialValue: '0',
            functionClass: 'IntFunction',
            isReference: true,
            isNumber: true,
            predicateClass: "IntPredicate",
            incFunctionMethod: 'newIncIntFunction')
    result.add new TxnObject(
            type: 'boolean',
            objectType: 'Boolean',
            referenceInterface: 'TxnBoolean',
            typeParameter: '',
            initialValue: 'false',
            functionClass: 'BooleanFunction',
            isReference: true,
            isNumber: false,
            predicateClass: "BooleanPredicate",
            incFunctionMethod: '')
    result.add new TxnObject(
            type: 'double',
            objectType: 'Double',
            referenceInterface: 'TxnDouble',
            typeParameter: '',
            initialValue: '0',
            functionClass: 'DoubleFunction',
            isReference: true,
            isNumber: true,
            predicateClass: "DoublePredicate",
            incFunctionMethod: '')
    result.add new TxnObject(
            referenceInterface: 'TxnLong',
            type: 'long',
            objectType: 'Long',
            typeParameter: '',
            initialValue: '0',
            functionClass: 'LongFunction',
            isReference: true,
            isNumber: true,
            predicateClass: "LongPredicate",
            incFunctionMethod: 'newIncLongFunction')
    result.add new TxnObject(
            type: '',
            objectType: '',
            typeParameter: '',
            initialValue: '',
            functionClass: 'Function',
            referenceInterface: '',
            isReference: false,
            isNumber: false,
            predicateClass: "",
            incFunctionMethod: '')
    result
}

void generateTxnCallable(VelocityEngine engine, TxnCallable callable) {
    Template t = engine.getTemplate('src/main/java/org/multiverse/api/callables/TxnCallable.vm')

    VelocityContext context = new VelocityContext()
    context.put('callable', callable)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File("src/main/java/org/multiverse/api/callables/${callable.name}.java")
    file.createNewFile()
    file.text = writer.toString()
}

void generateGammaTxnExecutor(VelocityEngine engine, TxnExecutor txnExecutor, List<TxnCallable> closures) {
    Template t = engine.getTemplate('src/main/java/org/multiverse/stms/gamma/GammaTxnExecutor.vm')

    VelocityContext context = new VelocityContext()
    context.put('txnExecutor', txnExecutor)
    context.put('callables', closures)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File("src/main/java/org/multiverse/stms/gamma/${txnExecutor.name}.java")
    file.createNewFile()
    file.text = writer.toString()
}

void generateTxnExecutor(VelocityEngine engine, List<TxnCallable> closures) {
    Template t = engine.getTemplate('src/main/java/org/multiverse/api/TxnExecutor.vm')

    VelocityContext context = new VelocityContext()
    context.put('callables', closures)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/api/TxnExecutor.java')
    file.createNewFile()
    file.text = writer.toString()
}

void generateOrElseBlock(VelocityEngine engine, List<TxnCallable> closures) {
    Template t = engine.getTemplate('src/main/java/org/multiverse/api/OrElseBlock.vm')

    VelocityContext context = new VelocityContext()
    context.put('callables', closures)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/api/OrElseBlock.java')
    file.createNewFile()
    file.text = writer.toString()
}

void generateGammaOrElseBlock(VelocityEngine engine, List<TxnCallable> closures) {
    Template t = engine.getTemplate('src/main/java/org/multiverse/stms/gamma/GammaOrElseBlock.vm')

    VelocityContext context = new VelocityContext()
    context.put('callables', closures)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/stms/gamma/GammaOrElseBlock.java')
    file.createNewFile()
    file.text = writer.toString()
}

void generateStmUtils(VelocityEngine engine, List<TxnCallable> callables) {
    Template t = engine.getTemplate('src/main/java/org/multiverse/api/StmUtils.vm')

    VelocityContext context = new VelocityContext()
    context.put('callables', callables)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/api/StmUtils.java')
    file.createNewFile()
    file.text = writer.toString()
}

void generatePredicate(VelocityEngine engine, TxnObject transactionalObject) {
    if (!transactionalObject.isReference) {
        return
    }

    Template t = engine.getTemplate('src/main/java/org/multiverse/api/predicates/Predicate.vm')

    VelocityContext context = new VelocityContext()
    context.put('transactionalObject', transactionalObject)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/api/predicates/', "${transactionalObject.predicateClass}.java")
    file.createNewFile()
    file.text = writer.toString()
}

void generateFunction(VelocityEngine engine, TxnObject transactionalObject) {
    if (!transactionalObject.isReference) {
        return
    }

    Template t = engine.getTemplate('src/main/java/org/multiverse/api/functions/Function.vm')

    VelocityContext context = new VelocityContext()
    context.put('transactionalObject', transactionalObject)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/api/functions/', "${transactionalObject.functionClass}.java")
    file.createNewFile()
    file.text = writer.toString()
}

void generateRefs(VelocityEngine engine, TxnObject transactionalObject) {
    if (!transactionalObject.isReference) {
        return;
    }

    Template t = engine.getTemplate('src/main/java/org/multiverse/api/references/TxnRef.vm');

    VelocityContext context = new VelocityContext()
    context.put('transactionalObject', transactionalObject)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/api/references/', "${transactionalObject.referenceInterface}.java")
    file.createNewFile()
    file.text = writer.toString()
}

void generateRefFactory(VelocityEngine engine, List<TxnObject> refs) {
    Template t = engine.getTemplate('src/main/java/org/multiverse/api/references/TxnRefFactory.vm');

    VelocityContext context = new VelocityContext()
    context.put('refs', refs)

    StringWriter writer = new StringWriter()
    t.merge(context, writer)

    File file = new File('src/main/java/org/multiverse/api/references/TxnRefFactory.java')
    file.createNewFile()
    file.text = writer.toString()
}

